using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using HostingEnvironment = System.Web.Hosting.HostingEnvironment;

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

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            // Count the number of slashes in the virtual path.
            int slashCount = 0;
            int countStartIndex = 0;
            while (true)
            {
                int index = this.ViewPath.IndexOf('/', countStartIndex);
                if (index == -1)
                    break;
                slashCount++;
                countStartIndex = index + 1;
            }

            // Construct a list of valid HTML file paths.
            var viewPath = HostingEnvironment.MapPath(this.ViewPath);
            var htmlPaths = new List<string>();
            htmlPaths.Add(HostingEnvironment.MapPath("~/Shared/Layout/Layout.html"));
            string baseDirectory = Path.GetDirectoryName(viewPath);
            if (slashCount >= 4)
                baseDirectory = Path.GetDirectoryName(baseDirectory);
            htmlPaths.Add(Path.Combine(baseDirectory, "Layout.html"));
            htmlPaths.Add(Path.Combine(baseDirectory, @"Layout\Layout.html"));
            htmlPaths.Add(viewPath);

            // Render the view.
            writer.Write(RenderFromCache(viewContext, htmlPaths));
        }

        private class RenderResult
        {
            public string Html;
            public string Css;
            public string Script;
            public List<object> Models;
            public RenderResult Clone()
            {
                return (RenderResult)this.MemberwiseClone();
            }
        }

        private class EmptyModel
        {
        }

        // File system watcher to invalidate the caches.
        private static FileSystemWatcher watcher;

        // Caches.
        private static System.Collections.Concurrent.ConcurrentDictionary<string, RenderResult> pageCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, RenderResult>();
        private static System.Collections.Concurrent.ConcurrentDictionary<string, Type> pageToModelTypeCache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, Type>();

        private string RenderFromCache(ViewContext viewContext, IEnumerable<string> htmlPaths)
        {
            // Initialize the file system watcher.
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(HostingEnvironment.ApplicationPhysicalPath);
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size;
                watcher.Changed += (sender, e) => { pageCache.Clear(); pageToModelTypeCache.Clear(); };
                watcher.Created += (sender, e) => { pageCache.Clear(); pageToModelTypeCache.Clear(); };
                watcher.Deleted += (sender, e) => { pageCache.Clear(); pageToModelTypeCache.Clear(); };
                watcher.Error += (sender, e) => { pageCache.Clear(); pageToModelTypeCache.Clear(); };
                watcher.Renamed += (sender, e) => { pageCache.Clear(); pageToModelTypeCache.Clear(); };
                watcher.EnableRaisingEvents = true;
            }

            // Attempt to read the page from the cache.
            RenderResult result;
            if (pageCache.TryGetValue(htmlPaths.Last(), out result) == false)
            {
                // The page isn't cached.
                result = Render(viewContext, htmlPaths.First(), htmlPaths.Skip(1));
                
                // Cache it.
                pageCache[htmlPaths.Last()] = result.Clone();

                // Replace the {{Model}} tag with the JSON model.
                return ReplaceModel(viewContext, result.Html, result.Models);
            }
            else
            {
                // Get a list of models.
                var controller = this.Controller as Lawspot.Controllers.MustacheController;
                if (controller == null)
                    throw new InvalidOperationException("Controllers must derive from MustacheController.");
                var models = new List<object>(htmlPaths.Count());
                foreach (var htmlPath in htmlPaths)
                {
                    Type modelType;
                    if (pageToModelTypeCache.TryGetValue(htmlPath, out modelType))
                    {
                        // Grab the model.
                        var model = controller.GetModel(viewContext, modelType);
                        if (modelType == null && model != null)
                            throw new InvalidOperationException(string.Format("The page {0} has no model declaration, but a model of type {1} was provided.",
                                ToVirtualPath(htmlPath), model.GetType().Name));
                        if (modelType != null && model == null)
                            throw new InvalidOperationException(string.Format("The page {0} expected a model of type {1} but no model was provided.",
                                ToVirtualPath(htmlPath), modelType.Name));
                        if (model != null && modelType != model.GetType())
                            throw new InvalidOperationException(string.Format("The page {0} expected a model of type {1} but the provided model was of type {2}.",
                                ToVirtualPath(htmlPath), modelType, model.GetType()));
                        models.Add(model ?? new EmptyModel());
                    }
                }

                // Replace the {{Model}} tag with the JSON model.
                return ReplaceModel(viewContext, result.Html, models);
            }
        }

        private RenderResult Render(ViewContext viewContext, string htmlPath, IEnumerable<string> childHtmlPaths)
        {
            // Ensure the controller derives from MustacheController.
            var controller = this.Controller as Lawspot.Controllers.MustacheController;
            if (controller == null)
                throw new InvalidOperationException("Controllers must derive from MustacheController.");

            // Render the children.
            RenderResult childResult = null;
            if (childHtmlPaths.Any())
                childResult = Render(viewContext, childHtmlPaths.First(), childHtmlPaths.Skip(1));

            // Check if the HTML file exists - if not, return the child result unmodified.
            if (File.Exists(htmlPath) == false)
                return childResult;

            var result = new RenderResult();

            // Render the stylesheets.
            string css = CombineAndMinify(viewContext, new CssMinify(), Path.ChangeExtension(htmlPath, ".css"),
                @"<style type=""text/css"">", "</style>", @"<link rel=""StyleSheet"" href=""{0}"" />");
            result.Css = childResult != null ? string.Concat(css, childResult.Css) : css;

            // Render the scripts.
            var script = CombineAndMinify(viewContext, new JsMinify(), Path.ChangeExtension(htmlPath, ".js"),
                @"<script type=""text/javascript"">", "</script>", @"<script src=""{0}""></script>");
            result.Script = childResult != null ? string.Concat(script, childResult.Script) : script;

            // Determine the model type.
            var html = File.ReadAllText(htmlPath);
            string modelTypeName = null;
            html = Regex.Replace(html, @"^\s*\{\{Model\s+([^}]+)\}\}\s*$", match =>
            {
                modelTypeName = match.Groups[1].Value;
                return string.Empty;
            }, RegexOptions.Multiline);
            Type modelType = null;
            if (modelTypeName != null)
            {
                // Look up the model type from the name.
                modelType = Type.GetType(modelTypeName);
                if (modelType == null)
                    throw new InvalidOperationException(string.Format("Type {0} could not be loaded (referenced on page {1}).",
                        modelTypeName, ToVirtualPath(htmlPath)));
            }

            // Cache the model type.
            pageToModelTypeCache[htmlPath] = modelType;

            // Replace the tags in the HTML.
            var htmlBuilder = new StringBuilder(html);
            htmlBuilder.Replace("{{Stylesheets}}", result.Css);
            if (childResult != null)
                htmlBuilder.Replace("{{Content}}", childResult.Html);
            htmlBuilder.Replace("{{Scripts}}", result.Script);
            result.Html = htmlBuilder.ToString();

            // Grab the model.
            var model = controller.GetModel(viewContext, modelType);
            if (modelType == null && model != null)
                throw new InvalidOperationException(string.Format("The page {0} has no model declaration, but a model of type {1} was provided.",
                    ToVirtualPath(htmlPath), model.GetType().Name));
            if (modelType != null && model == null)
                throw new InvalidOperationException(string.Format("The page {0} expected a model of type {1} but no model was provided.",
                    ToVirtualPath(htmlPath), modelType.Name));
            if (model != null && modelType != model.GetType())
                throw new InvalidOperationException(string.Format("The page {0} expected a model of type {1} but the provided model was of type {2}.",
                    ToVirtualPath(htmlPath), modelType, model.GetType()));
            result.Models = new List<object>();
            result.Models.Add(model ?? new EmptyModel());
            if (childResult != null)
                result.Models.AddRange(childResult.Models);

            return result;
        }

        private string ReplaceModel(ViewContext viewContext, string html, List<object> models)
        {
            var htmlBuilder = new StringBuilder(html);

            var modelHtml = new StringBuilder();
            modelHtml.AppendLine(@"<script type=""text/javascript"">");
            modelHtml.Append("var Model = ");
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
            jsonSerializer.ContractResolver = new ViewModelContractResolver(
                models.Last().GetType(),
                models.Take(models.Count - 1).Select(m => m.GetType()).ToArray(),
                models.Take(models.Count - 1).ToArray());
            jsonSerializer.Serialize(new StringWriter(modelHtml), models.Last());
            modelHtml.AppendLine(";");
            modelHtml.AppendLine("</script>");
            htmlBuilder.Replace("{{Model}}", modelHtml.ToString());

            // Tack on timing information.
            htmlBuilder.AppendFormat("<!-- Took {0} ms -->", ((System.Diagnostics.Stopwatch)viewContext.HttpContext.Items["Stopwatch"]).Elapsed.TotalMilliseconds);

            return htmlBuilder.ToString();
        }

        private static string ToVirtualPath(string path)
        {
            return string.Format("~/{0}", path.Replace(HostingEnvironment.ApplicationPhysicalPath, "").Replace("\\", "/"));
        }

        /// <summary>
        /// Implements a custom contract resolver to add the extra properties into the JSON object.
        /// </summary>
        private class ViewModelContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            private Type baseType;
            private Type[] additionalTypes;
            private object[] additionalValues;

            /// <summary>
            /// Constructs a new ViewModelContractResolver.
            /// </summary>
            /// <param name="baseType"> The type to insert extra members into. </param>
            /// <param name="additionalTypes"> The types to insert. </param>
            /// <param name="additionalValues"> The values corresponding to the additional values. </param>
            public ViewModelContractResolver(Type baseType, Type[] additionalTypes, object[] additionalValues)
            {
                this.baseType = baseType;
                this.additionalTypes = additionalTypes;
                this.additionalValues = additionalValues;
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
                    var result = new List<System.Reflection.MemberInfo>();
                    foreach (var type in this.additionalTypes)
                        result.AddRange(base.GetSerializableMembers(type));
                    result.AddRange(base.GetSerializableMembers(this.baseType));
                    return result;
                }
                return base.GetSerializableMembers(objectType);
            }

            /// <summary>
            /// A custom IValueProvider that pulls values from the constructor.
            /// </summary>
            private class ValueProvider : Newtonsoft.Json.Serialization.IValueProvider
            {
                private Newtonsoft.Json.Serialization.DynamicValueProvider implementation;
                private object value;

                public ValueProvider(System.Reflection.MemberInfo memberInfo, object value)
                {
                    this.implementation = new Newtonsoft.Json.Serialization.DynamicValueProvider(memberInfo);
                    this.value = value;
                }

                public object GetValue(object target)
                {
                    return this.implementation.GetValue(value);
                }

                public void SetValue(object target, object value)
                {
                    this.implementation.SetValue(this.value, value);
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
                for (int i = 0; i < this.additionalTypes.Length; i ++)
                {
                    var type = this.additionalTypes[i];
                    if (member.DeclaringType == type)
                        property.ValueProvider = new ValueProvider(member, this.additionalValues[i]);
                }
                return property;
            }
        }

        private string CombineAndMinify(ViewContext viewContext, IBundleTransform minifier, string path, string startTag, string endTag, string linkTag)
        {
            if (viewContext == null)
                throw new ArgumentNullException("viewContext");
            if (minifier == null)
                throw new ArgumentNullException("minifier");
            if (path == null)
                throw new ArgumentNullException("path");

            // Check if the file exists.
            if (File.Exists(path) == false)
                return string.Empty;

            // Read the file.
            string originalContent = File.ReadAllText(path);

            // Detect any includes.
            List<string> fileIncludes = new List<string>();
            List<string> urlIncludes = new List<string>();
            originalContent = Regex.Replace(originalContent, @"\{\{Include\s+([^}]+)\}\}", match =>
                {
                    var includePath = match.Groups[1].Value;
                    if (includePath.Contains('/'))
                    {
                        // Path is a URL.
                        urlIncludes.Add(includePath);
                    }
                    else
                    {
                        // Path is a file.
                        fileIncludes.Add(Path.Combine(Path.GetDirectoryName(path), includePath));
                    }
                    return string.Empty;
                }, RegexOptions.Multiline);

            // Read the contents of each file.
            var content = new StringBuilder();
            var fileInfos = new List<FileInfo>(fileIncludes.Count);
            foreach (var fileInclude in fileIncludes)
            {
                fileInfos.Add(new FileInfo(fileInclude));
                content.AppendLine(File.ReadAllText(fileInclude));
            }
            content.Append(originalContent);
            fileInfos.Add(new FileInfo(path));

            // Create a bundle.
            var bundle = new BundleResponse(content.ToString(), fileInfos);
            
            // Use the minifier on the bundle.
            minifier.Process(new BundleContext(viewContext.HttpContext, BundleTable.Bundles, "~/"), bundle);

            // Now tack on HTML and URL includes.
            var result = new StringBuilder();
            foreach (var urlInclude in urlIncludes)
                result.AppendLine(string.Format(linkTag, urlInclude));
            result.AppendLine(startTag);
            result.AppendLine(bundle.Content);
            result.AppendLine(endTag);
            return result.ToString();
        }
    }
}

