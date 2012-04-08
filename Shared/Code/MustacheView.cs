using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace Lawspot.Shared
{
    public class MustacheView : IView
    {
        public MustacheView(ControllerBase controller, string viewPath)
        {
            this.Controller = controller;
            this.ViewPath = viewPath;
        }

        public ControllerBase Controller { get; private set; }
        public string ViewPath { get; private set; }

        private class EmptyModel
        {
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var server = viewContext.HttpContext.Server;

            // First we load the layout.
            var layout = new StringBuilder(File.ReadAllText(server.MapPath("~/Shared/Layout.html")));

            // Render the stylesheets.
            var stylesheetsHtml = new StringBuilder();
            stylesheetsHtml.AppendLine(@"<style type=""text/css"">");
            stylesheetsHtml.AppendLine(CombineAndMinify(viewContext, ".css", new CssMinify(), true, true));
            stylesheetsHtml.AppendLine("</style>");

            // Replace the {{Stylesheets}} tag.
            layout.Replace("{{Stylesheets}}", stylesheetsHtml.ToString());

            // Replace the {{Content}} tag.
            layout.Replace("{{Content}}", File.ReadAllText(server.MapPath(this.ViewPath)));

            // Replace the {{Model}}
            var model = this.Controller.ViewData.Model ?? new EmptyModel();

            // Initialize the extra state that gets tacked on before the view model state.
            var extraState = new ExtraViewModelState();

            // User.
            extraState.User = ((Controller)viewContext.Controller).User as CustomPrincipal;

            // Translate alert types into messages.
            switch (viewContext.HttpContext.Request.QueryString["alert"])
            {
                case "loggedin":
                    extraState.SuccessMessage = "You're logged in. Welcome back to LawSpot.";
                    break;
                case "loggedout":
                    extraState.SuccessMessage = "You have logged out.";
                    break;
                case "registered":
                case "registered-as-lawyer":
                    extraState.SuccessMessage = string.Format("Thanks for registering!  Please check your email ({0}) to confirm your account with us.", extraState.User.EmailAddress);
                    break;
            }

            // ModelState.
            var result = new Dictionary<string, object>();
            var modelState = ((Controller)viewContext.Controller).ModelState;
            foreach (var key in modelState.Keys)
            {
                var state = modelState[key];
                if (state.Errors.Count > 0)
                {
                    string errorMessage = string.Join(Environment.NewLine, state.Errors.Select(e => e.ErrorMessage));
                    var dictionary = result;
                    var keys = key.Split('.');
                    for (int i = 0; i < keys.Length - 1; i ++)
                    {
                        object subDictionary;
                        if (dictionary.TryGetValue(keys[i], out subDictionary) == false)
                            dictionary[keys[i]] = new Dictionary<string, object>();
                        dictionary = (Dictionary<string, object>)dictionary[keys[i]];
                    }
                    dictionary.Add(keys[keys.Length - 1], errorMessage);
                }
            }
            if (result.Count > 0)
                extraState.ModelState = result;

            // Store the extra state in the HttpContext object where it can be retrieved by the contract resolver.
            viewContext.HttpContext.Items["ExtraState"] = extraState;

            var modelHtml = new StringBuilder();
            modelHtml.AppendLine(@"<script type=""text/javascript"">");
            modelHtml.Append("var Model = ");
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            jsonSerializer.ContractResolver = new ViewModelContractResolver(model.GetType());
            jsonSerializer.Serialize(new StringWriter(modelHtml), model);
            modelHtml.AppendLine(";");
            modelHtml.AppendLine("</script>");
            layout.Replace("{{Model}}", modelHtml.ToString());

            // Next we find all the script files.
            var scripts = new List<string>();
            scripts.Add(server.MapPath("~/Shared/Layout.js"));
            scripts.Add(Path.ChangeExtension(server.MapPath(this.ViewPath), "js"));

            // Render the scripts (1).
            var scriptsHtml = new StringBuilder();
            scriptsHtml.AppendLine(@"<script type=""text/javascript"">");
            scriptsHtml.AppendLine(CombineAndMinify(viewContext, ".js", new JsMinify(), true, false));
            scriptsHtml.AppendLine(@"</script>");

            // Replace the {{LayoutScripts}}
            layout.Replace("{{LayoutScripts}}", scriptsHtml.ToString());

            // Render the scripts (2).
            scriptsHtml = new StringBuilder();
            scriptsHtml.AppendLine(@"<script type=""text/javascript"">");
            scriptsHtml.AppendLine(CombineAndMinify(viewContext, ".js", new JsMinify(), false, true));
            scriptsHtml.AppendLine(@"</script>");

            // Replace the {{PageScripts}}
            layout.Replace("{{PageScripts}}", scriptsHtml.ToString());

            writer.Write(layout);
        }

        /// <summary>
        /// The extra properties to add onto the JSON view model object.
        /// </summary>
        private class ExtraViewModelState
        {
            /// <summary>
            /// The currently logged in user.
            /// </summary>
            public CustomPrincipal User { get; set; }

            /// <summary>
            /// The model state.
            /// </summary>
            public IDictionary<string, object> ModelState { get; set; }

            /// <summary>
            /// A green "all systems go" message.
            /// </summary>
            public string SuccessMessage { get; set; }
        }

        /// <summary>
        /// Implements a custom contract resolver to add the extra properties into the JSON object.
        /// </summary>
        private class ViewModelContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            private Type baseType;

            /// <summary>
            /// Constructs a new ViewModelContractResolver.
            /// </summary>
            /// <param name="baseType"> The type to insert extra members into. </param>
            public ViewModelContractResolver(Type baseType)
            {
                this.baseType = baseType;
            }

            /// <summary>
            /// Gets the serializable members for the type.
            /// </summary>
            /// <param name="objectType">The type to get serializable members for.</param>
            /// <returns>The serializable members for the type.</returns>
            protected override List<System.Reflection.MemberInfo> GetSerializableMembers(Type objectType)
            {
                if (objectType == this.baseType)
                {
                    var result = base.GetSerializableMembers(typeof(ExtraViewModelState));
                    result.AddRange(base.GetSerializableMembers(objectType));
                    return result;
                }
                return base.GetSerializableMembers(objectType);
            }

            /// <summary>
            /// A custom IValueProvider that pulls values from HttpContext.
            /// </summary>
            private class ExtraStateValueProvider : Newtonsoft.Json.Serialization.IValueProvider
            {
                private Newtonsoft.Json.Serialization.DynamicValueProvider implementation;

                public ExtraStateValueProvider(System.Reflection.MemberInfo memberInfo)
                {
                    this.implementation = new Newtonsoft.Json.Serialization.DynamicValueProvider(memberInfo);
                }

                public object GetValue(object target)
                {
                    return this.implementation.GetValue(HttpContext.Current.Items["ExtraState"]);
                }

                public void SetValue(object target, object value)
                {
                    this.implementation.SetValue(HttpContext.Current.Items["ExtraState"], value);
                }
            }

            /// <summary>
            /// Overrides the property value provider so that values come from the HttpContext.
            /// </summary>
            /// <param name="member"></param>
            /// <param name="memberSerialization"></param>
            /// <returns></returns>
            protected override Newtonsoft.Json.Serialization.JsonProperty CreateProperty(System.Reflection.MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (member.DeclaringType == typeof(ExtraViewModelState))
                    property.ValueProvider = new ExtraStateValueProvider(member);
                return property;
            }
        }

        private string CombineAndMinify(ViewContext viewContext, string extension, IBundleTransform minifier, bool shared, bool page)
        {
            if (viewContext == null)
                throw new ArgumentNullException("viewContext");
            if (extension == null)
                throw new ArgumentNullException("extension");
            if (extension.StartsWith(".") == false)
                throw new ArgumentException("extension must begin with a period.", "extension");
            if (minifier == null)
                throw new ArgumentNullException("minifier");

            var paths = new List<string>();

            // Find all the .js or .css files in the shared folder.
            if (shared)
            {
                var sharedDirectory = viewContext.HttpContext.Server.MapPath("~/Shared/");
                paths.AddRange(Directory.EnumerateFiles(sharedDirectory, "*" + extension));
            }

            // Count the number of slashes in the virtual path.
            int slashCount = 0;
            int countStartIndex = 0;
            while (true)
            {
                int index = this.ViewPath.IndexOf('/', countStartIndex);
                if (index == -1)
                    break;
                slashCount ++;
                countStartIndex = index + 1;
            }

            if (page)
            {
                if (slashCount <= 3)
                {
                    // Find the related .js or .css file.
                    paths.Add(Path.ChangeExtension(viewContext.HttpContext.Server.MapPath(this.ViewPath), extension));
                }
                else
                {
                    // Find all the .js or .css files in the page folder.
                    string viewPath = viewContext.HttpContext.Server.MapPath(this.ViewPath);
                    paths.AddRange(Directory.EnumerateFiles(Path.GetDirectoryName(viewPath), "*" + extension));
                }
            }

            // Read the contents of each file.
            var content = new StringBuilder();
            var fileInfos = new List<FileInfo>(paths.Count);
            foreach (var path in paths)
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    fileInfos.Add(fileInfo);
                    content.AppendLine(File.ReadAllText(fileInfo.FullName));
                }
            }

            // Create a bundle.
            var bundle = new BundleResponse(content.ToString(), fileInfos);
            
            // Use the minifier on the bundle.
            minifier.Process(new BundleContext(viewContext.HttpContext, BundleTable.Bundles, "~/"), bundle);

            return bundle.Content;
        }
    }
}

