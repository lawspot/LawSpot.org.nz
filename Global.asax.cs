using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;

namespace Lawspot
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Execute any migrations.
            Lawspot.Backend.LawspotDataContext.ExecuteMigrations();

            // Remove the ASP.NET ViewEngine and add my own.
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new Lawspot.Shared.MustacheViewEngine());

            // Register areas.
            AreaRegistration.RegisterAllAreas();

            // Register filters.
            GlobalFilters.Filters.Add(new HandleErrorAttribute());

            // Files at root.
            RouteTable.Routes.IgnoreRoute("favicon.ico");
            RouteTable.Routes.IgnoreRoute("robots.txt");

            // Register routes.
            RouteTable.Routes.MapRoute("HomeRoute", "home", new { controller = "Browse", action = "Home" });
            RouteTable.Routes.MapRoute("BrowseRoute", "browse", new { controller = "Browse", action = "Browse" });
            RouteTable.Routes.MapRoute("LoginRoute", "login", new { controller = "Account", action = "Login" });
            RouteTable.Routes.MapRoute("LogoutRoute", "logout", new { controller = "Account", action = "Logout" });
            RouteTable.Routes.MapRoute("RegisterRoute", "register", new { controller = "Account", action = "Register" });
            RouteTable.Routes.MapRoute("LawyerRegisterRoute", "lawyer-register", new { controller = "Account", action = "LawyerRegister" });
            RouteTable.Routes.MapRoute("LawyerRegisterThankYouRoute", "lawyer-thank-you", new { controller = "Account", action = "LawyerThankYou" });
            RouteTable.Routes.MapRoute("ValidateEmailAddressRoute", "validate-email", new { controller = "Account", action = "ValidateEmailAddress" });
            RouteTable.Routes.MapRoute("AskRoute", "ask", new { controller = "Ask", action = "Ask" });
            RouteTable.Routes.MapRoute("HowItWorksRoute", "how-it-works", new { controller = "Static", action = "HowItWorks" });
            RouteTable.Routes.MapRoute("FAQRoute", "faq", new { controller = "Static", action = "FAQ" });
            RouteTable.Routes.MapRoute("AboutRoute", "about", new { controller = "Static", action = "AboutUs" });
            RouteTable.Routes.MapRoute("PrivacyRoute", "privacy-policy", new { controller = "Static", action = "Privacy" });
            RouteTable.Routes.MapRoute("TermsRoute", "terms", new { controller = "Static", action = "TermsOfUse" });
            RouteTable.Routes.MapRoute("LawyerPolicyRoute", "lawyer-policy", new { controller = "Static", action = "LawyerPolicy" });
            RouteTable.Routes.MapRoute("AnswerPolicyRoute", "answer-policy", new { controller = "Static", action = "AnswerPolicy" });
            RouteTable.Routes.MapRoute("ImportantNoticeRoute", "important-notice", new { controller = "Static", action = "ImportantNotice" });
            RouteTable.Routes.MapRoute("PartnerRoute", "partner", new { controller = "Static", action = "PartnerWithUs" });
            RouteTable.Routes.MapRoute("ContactUsRoute", "contact", new { controller = "Static", action = "ContactUs" });
            RouteTable.Routes.MapRoute("AskThankYouRoute", "ask/thank-you", new { controller = "Ask", action = "ThankYou" });
            RouteTable.Routes.MapRoute("AdminAnswerQuestionsRoute", "admin/answer-questions", new { controller = "Admin", action = "AnswerQuestions" });
            RouteTable.Routes.MapRoute("AdminPostAnswerRoute", "admin/post-answer", new { controller = "Admin", action = "PostAnswer" });
            RouteTable.Routes.MapRoute("AdminReviewLawyersRoute", "admin/review-lawyers", new { controller = "Admin", action = "ReviewLawyers" });
            RouteTable.Routes.MapRoute("AdminPostRejectLawyerRoute", "admin/post-reject-lawyer", new { controller = "Admin", action = "RejectLawyer" });
            RouteTable.Routes.MapRoute("AdminPostApproveLawyerRoute", "admin/post-approve-lawyer", new { controller = "Admin", action = "ApproveLawyer" });
            RouteTable.Routes.MapRoute("AdminReviewQuestionsRoute", "admin/review-questions", new { controller = "Admin", action = "ReviewQuestions" });
            RouteTable.Routes.MapRoute("AdminPostRejectQuestionRoute", "admin/post-reject-question", new { controller = "Admin", action = "RejectQuestion" });
            RouteTable.Routes.MapRoute("AdminPostApproveQuestionRoute", "admin/post-approve-question", new { controller = "Admin", action = "ApproveQuestion" });
            RouteTable.Routes.MapRoute("AdminReviewAnswersRoute", "admin/review-answers", new { controller = "Admin", action = "ReviewAnswers" });
            RouteTable.Routes.MapRoute("AdminPostRejectAnswerRoute", "admin/post-reject-answer", new { controller = "Admin", action = "RejectAnswer" });
            RouteTable.Routes.MapRoute("AdminPostApproveAnswerRoute", "admin/post-approve-answer", new { controller = "Admin", action = "ApproveAnswer" });
            RouteTable.Routes.MapRoute("AdminAccountSettingsRoute", "admin/account-settings", new { controller = "Admin", action = "AccountSettings" });
            RouteTable.Routes.MapRoute("CategoryRoute", "{slug}", new { controller = "Browse", action = "Category" });
            RouteTable.Routes.MapRoute("QuestionRoute", "{category}/{slug}", new { controller = "Browse", action = "Question" });
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
                this.Context.User = Lawspot.Shared.CustomPrincipal.FromTicket(ticket);
            }
        }

        protected void Application_BeginRequest()
        {
            // Stash a stopwatch in the request items so we can record the page generation time.
            HttpContext.Current.Items["Stopwatch"] = System.Diagnostics.Stopwatch.StartNew();
        }

        protected void Application_EndRequest()
        {
            // Dispose of the data context.
            Lawspot.Controllers.BaseController.DisposeDataContext();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();
            Lawspot.Shared.Logger.LogError(ex);
        } 
    }
}