using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Shared;

namespace Lawspot.Controllers
{
    [ValidateInput(false)]
    public class BaseController : MustacheController
    {
        /// <summary>
        /// Gets a reference to the EF database context.
        /// </summary>
        public DataClassesDataContext DataContext
        {
            get
            {
                var dataContext = (LawspotDataContext)this.HttpContext.Items["DataContext"];
                if (dataContext == null)
                {
                    dataContext = new LawspotDataContext();
                    this.HttpContext.Items["DataContext"] = dataContext;
                }
                return dataContext;
            }
        }

        /// <summary>
        /// Disposes of any resources used by the current data context.
        /// </summary>
        public static void DisposeDataContext()
        {
            var dataContext = (LawspotDataContext)System.Web.HttpContext.Current.Items["DataContext"];
            if (dataContext != null)
            {
                dataContext.Dispose();
                System.Web.HttpContext.Current.Items["DataContext"] = null;
            }
        }

        /// <summary>
        /// Overrides the User property for easier access.
        /// </summary>
        public new CustomPrincipal User
        {
            get { return base.User as CustomPrincipal; }
        }

        /// <summary>
        /// The extra properties to add onto the JSON view model object.
        /// </summary>
        private class LayoutViewModel
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
        /// Gets a model object of a given type.
        /// </summary>
        /// <param name="viewContext"> The view context. </param>
        /// <param name="modelType"> The type of model to return. </param>
        /// <returns> A model of the given type. </returns>
        protected internal override object GetModel(ViewContext viewContext, Type modelType)
        {
            if (modelType == typeof(LayoutViewModel))
            {
                // Initialize the extra state that gets tacked on before the view model state.
                var model = new LayoutViewModel();

                // User.
                model.User = ((Controller)viewContext.Controller).User as CustomPrincipal;

                // Translate alert types into messages.
                switch (viewContext.HttpContext.Request.QueryString["alert"])
                {
                    case "loggedin":
                        model.SuccessMessage = "You're logged in. Welcome back to LawSpot.";
                        break;
                    case "loggedout":
                        model.SuccessMessage = "You have logged out.";
                        break;
                    case "registered":
                        model.SuccessMessage = string.Format("Thanks for registering!  Please check your email ({0}) to confirm your account with us.", this.User.EmailAddress);
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
                        for (int i = 0; i < keys.Length - 1; i++)
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
                    model.ModelState = result;
                return model;
            }
            return base.GetModel(viewContext, modelType);
        }

        /// <summary>
        /// Creates an authentication cookie on the user's computer.
        /// </summary>
        /// <param name="user"> The user details. </param>
        /// <param name="rememberMe"> <c>true</c> to make the cookie persistant. </param>
        protected void Login(User user, bool rememberMe)
        {
            var ticket = Lawspot.Shared.CustomPrincipal.FromUser(user).ToTicket(rememberMe);
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (rememberMe)
                cookie.Expires = DateTime.Now.AddYears(1);  // Remember Me = good for one year.
            this.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Utility method to set a query string parameter in a URL.
        /// </summary>
        /// <param name="uri"> The URL to change. </param>
        /// <param name="key"> The parameter key. </param>
        /// <param name="value"> The parameter value. </param>
        /// <returns> The provided URL, with the given parameter set to the given value. </returns>
        protected Uri SetUriParameter(Uri uri, string key, string value)
        {
            var builder = new UriBuilder(uri);
            var parameters = HttpUtility.ParseQueryString(builder.Query);
            parameters[key] = value;
            builder.Query = parameters.ToString();
            return builder.Uri;
        }
    }
}