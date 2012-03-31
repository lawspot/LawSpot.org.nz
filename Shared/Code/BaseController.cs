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
    public class BaseController : Controller
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