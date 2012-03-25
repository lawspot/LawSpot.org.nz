using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Prolawyers
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Remove the ASP.NET ViewEngine and add my own.
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new Prolawyers.Shared.MustacheViewEngine());

            // Register areas.
            AreaRegistration.RegisterAllAreas();

            // Register filters.
            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            // Register routes.
            RouteTable.Routes.MapRoute("HomeRoute", "", new { controller = "Home", action = "Index" });
            RouteTable.Routes.MapRoute("LoginRoute", "login", new { controller = "Account", action = "Login" });
            RouteTable.Routes.MapRoute("LogoutRoute", "logout", new { controller = "Account", action = "Logout" });
            RouteTable.Routes.MapRoute("RegisterRoute", "register", new { controller = "Account", action = "Register" });
            RouteTable.Routes.MapRoute("LawyerRegisterRoute", "lawyer-register", new { controller = "Account", action = "LawyerRegister" });
        }

        protected void Application_AuthorizeRequest()
        {
            // Check there is an authentication cookie.
            if (Request.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName))
            {
                // Decrypt the cookie.
                var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                var ticket = FormsAuthentication.Decrypt(cookie.Value);

                // Extract the user information.
                this.Context.User = Prolawyers.Shared.CustomPrincipal.FromTicket(ticket);
            }
        }

        protected void Application_BeginRequest()
        {
        }

        protected void Application_EndRequest()
        {
            // Dispose of the data context.
            Prolawyers.Controllers.BaseController.DisposeDataContext();
        }
    }
}