using System;
using System.Collections.Generic;
using System.Configuration;
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

            // Files at root.
            RouteTable.Routes.IgnoreRoute("favicon.ico");
            RouteTable.Routes.IgnoreRoute("robots.txt");

            // Register routes.
            RouteTable.Routes.MapRoute("HomeRoute2", "home", new { controller = "Browse", action = "Home" });
            RouteTable.Routes.MapRoute("BrowseRoute", "browse", new { controller = "Browse", action = "Browse" });
            RouteTable.Routes.MapRoute("SearchRoute", "search", new { controller = "Browse", action = "Search" });
            RouteTable.Routes.MapRoute("LoginRoute", "login", new { controller = "Account", action = "Login" });
            RouteTable.Routes.MapRoute("LogoutRoute", "logout", new { controller = "Account", action = "Logout" });
            RouteTable.Routes.MapRoute("RegisterRoute", "register", new { controller = "Account", action = "Register" });
            RouteTable.Routes.MapRoute("ResetPasswordRoute", "resetpassword", new { controller = "Account", action = "ResetPassword" });
            RouteTable.Routes.MapRoute("LawyerRegisterRoute", "lawyer-register", new { controller = "Account", action = "LawyerRegister" });
            RouteTable.Routes.MapRoute("LawyerRegisterThankYouRoute", "lawyer-thank-you", new { controller = "Account", action = "LawyerThankYou" });
            RouteTable.Routes.MapRoute("ValidateEmailAddressRoute", "validate-email", new { controller = "Account", action = "ValidateEmailAddress" });
            RouteTable.Routes.MapRoute("AskRoute", "ask", new { controller = "Ask", action = "Ask" });
            RouteTable.Routes.MapRoute("SuggestionsRoute", "suggestions", new { controller = "Ask", action = "Suggestions" });
            RouteTable.Routes.MapRoute("HowItWorksRoute", "how-it-works", new { controller = "Static", action = "HowItWorks" });
            RouteTable.Routes.MapRoute("FAQRoute", "faq", new { controller = "Static", action = "FAQ" });
            RouteTable.Routes.MapRoute("AboutRoute", "about", new { controller = "Static", action = "AboutUs" });
            RouteTable.Routes.MapRoute("PrivacyRoute", "privacy-policy", new { controller = "Static", action = "Privacy" });
            RouteTable.Routes.MapRoute("TermsRoute", "terms", new { controller = "Static", action = "TermsOfUse" });
            RouteTable.Routes.MapRoute("LawyerPolicyRoute", "lawyer-policy", new { controller = "Static", action = "LawyerPolicy" });
            RouteTable.Routes.MapRoute("ImportantNoticeRoute", "important-notice", new { controller = "Static", action = "ImportantNotice" });
            RouteTable.Routes.MapRoute("PartnerRoute", "partner", new { controller = "Static", action = "PartnerWithUs" });
            RouteTable.Routes.MapRoute("ContactUsRoute", "contact", new { controller = "Static", action = "ContactUs" });
            RouteTable.Routes.MapRoute("404Route", "404", new { controller = "Static", action = "Error404" });
            RouteTable.Routes.MapRoute("500Route", "500", new { controller = "Static", action = "Error500" });
            RouteTable.Routes.MapRoute("AskThankYouRoute", "ask/thank-you", new { controller = "Ask", action = "ThankYou" });
            RouteTable.Routes.MapRoute("AdminActivityStreamRoute", "admin/activity-stream", new { controller = "Admin", action = "ActivityStream" });
            RouteTable.Routes.MapRoute("AdminAnswerQuestionsRoute", "admin/answer-questions", new { controller = "Admin", action = "AnswerQuestions" });
            RouteTable.Routes.MapRoute("AdminCheckQuestionStatusRoute", "admin/check-question-status", new { controller = "Admin", action = "CheckQuestionStatus" });
            RouteTable.Routes.MapRoute("AdminSaveDraftAnswerRoute", "admin/save-draft-answer", new { controller = "Admin", action = "SaveDraftAnswer" });
            RouteTable.Routes.MapRoute("AdminPostAnswerRoute", "admin/post-answer", new { controller = "Admin", action = "PostAnswer" });
            RouteTable.Routes.MapRoute("AdminReviewLawyersRoute", "admin/review-lawyers", new { controller = "Admin", action = "ReviewLawyers" });
            RouteTable.Routes.MapRoute("AdminPostRejectLawyerRoute", "admin/post-reject-lawyer", new { controller = "Admin", action = "RejectLawyer" });
            RouteTable.Routes.MapRoute("AdminPostApproveLawyerRoute", "admin/post-approve-lawyer", new { controller = "Admin", action = "ApproveLawyer" });
            RouteTable.Routes.MapRoute("AdminReviewQuestionsRoute", "admin/review-questions", new { controller = "Admin", action = "ReviewQuestions" });
            RouteTable.Routes.MapRoute("AdminPostRejectQuestionRoute", "admin/post-reject-question", new { controller = "Admin", action = "RejectQuestion" });
            RouteTable.Routes.MapRoute("AdminPostApproveQuestionRoute", "admin/post-approve-question", new { controller = "Admin", action = "ApproveQuestion" });
            RouteTable.Routes.MapRoute("AdminReviewAnswersRoute", "admin/review-answers", new { controller = "Admin", action = "ReviewAnswers" });
            RouteTable.Routes.MapRoute("AdminReviewAnswerRoute", "admin/review-answer", new { controller = "Admin", action = "ReviewAnswer" });
            RouteTable.Routes.MapRoute("AdminPostRejectAnswerRoute", "admin/post-reject-answer", new { controller = "Admin", action = "RejectAnswer" });
            RouteTable.Routes.MapRoute("AdminPostApproveAnswerRoute", "admin/post-approve-answer", new { controller = "Admin", action = "ApproveAnswer" });
            RouteTable.Routes.MapRoute("AdminAccountSettingsRoute", "admin/account-settings", new { controller = "Admin", action = "AccountSettings" });
            RouteTable.Routes.MapRoute("AdminVetterPolicyRoute", "admin/vetter-policy", new { controller = "Admin", action = "VetterPolicy" });
            RouteTable.Routes.MapRoute("AdminAdminRoute", "admin/admin", new { controller = "Admin", action = "Admin" });
            RouteTable.Routes.MapRoute("TasksTestRoute", "scheduled-tasks/test", new { controller = "Tasks", action = "Test" });
            RouteTable.Routes.MapRoute("TasksSendReminderEmailsRoute", "scheduled-tasks/send-reminder-emails", new { controller = "Tasks", action = "SendReminderEmails" });
            RouteTable.Routes.MapRoute("CategoryRoute", "{slug}", new { controller = "Browse", action = "Category" });
            RouteTable.Routes.MapRoute("QuestionRoute", "{category}/{slug}", new { controller = "Browse", action = "Question" });
            RouteTable.Routes.MapRoute("HomeRoute", "", new { controller = "Browse", action = "Home" });
        }

        protected void Application_AuthenticateRequest()
        {
            // By default, no user.
            this.Context.User = null;

            // Check there is an authentication cookie.
            if (Request.Cookies.AllKeys.Contains(FormsAuthentication.FormsCookieName))
            {
                // Decrypt the cookie.
                var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];

                try
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);

                    // Ensure the ticket hasn't expired.
                    if (ticket.Expiration > DateTime.Now)
                    {
                        // Extract the user information.
                        var principal = Lawspot.Shared.CustomPrincipal.FromTicket(ticket);
                        this.Context.User = principal;

                        // If the ticket has less than half the time remaining, issue a new one
                        // (sliding expiration).
                        if (ticket.Expiration.Subtract(DateTime.Now) <
                            TimeSpan.FromMinutes(ticket.Expiration.Subtract(ticket.IssueDate).TotalMinutes / 2))
                        {
                            // Re-issue.
                            this.Response.Cookies.Add(principal.ToCookie(ticket.IsPersistent));
                        }
                    }
                    else
                    {
                        // The ticket has expired.
                        FormsAuthentication.SignOut();
                        this.Context.Items.Add("SessionExpired", true);
                    }
                }
                catch (Exception ex)
                {
                    // FormsAuthentication.Decrypt can fail if the cookie is bad or the machine key
                    // has changed.
                    Lawspot.Shared.Logger.LogError(ex);
                    FormsAuthentication.SignOut();
                }
            }
        }

        protected void Application_BeginRequest()
        {
            // Stash a stopwatch in the request items so we can record the page generation time.
            this.Context.Items.Add("Stopwatch", System.Diagnostics.Stopwatch.StartNew());
        }

        protected void Application_EndRequest()
        {
            // Dispose of the data context.
            Lawspot.Controllers.BaseController.DisposeDataContext();

            // If the response is 401, then redirect to the login page.
            if (this.Response.StatusCode == 401)
            {
                var redirectUrl = "~/login?returnUrl=" + Uri.EscapeDataString(this.Request.RawUrl);
                if (this.Context.Items.Contains("SessionExpired"))
                    redirectUrl += "&sessionExpired=true";
                Response.Redirect(redirectUrl);
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Get a reference to the error.
            var ex = Server.GetLastError().GetBaseException();

            // Determine the status code to return.
            int statusCode = 500;
            if (ex is HttpException)
                statusCode = ((HttpException)ex).GetHttpCode();

            // Log everything except 404s originating from third-party sites.
            bool firstPartyReferrer = HttpContext.Current != null && HttpContext.Current.Request.UrlReferrer != null &&
                string.Equals(HttpContext.Current.Request.UrlReferrer.Host, ConfigurationManager.AppSettings["DomainName"]);
            if (statusCode != 404 || firstPartyReferrer)
                Lawspot.Shared.Logger.LogError(ex);

            // Determine the URL to redirect to.
            var redirectUrl = "/500";
            if (this.Context.IsCustomErrorEnabled)
            {
                var section = (System.Web.Configuration.CustomErrorsSection)System.Configuration.ConfigurationManager.GetSection("system.web/customErrors");
                if (section != null)
                {
                    redirectUrl = section.DefaultRedirect;
                    var errorElement = section.Errors[statusCode.ToString()];
                    if (errorElement != null)
                        redirectUrl = errorElement.Redirect;
                }

                // Send the response to the client.
                this.Context.Response.StatusCode = statusCode;
                this.Context.Response.TrySkipIisCustomErrors = true;
                this.Context.ClearError();

                // Render a view.
                var httpContext = new HttpContextWrapper(this.Context);
                this.Context.RewritePath(redirectUrl, false);
                var routeData = RouteTable.Routes.GetRouteData(httpContext);
                var mvcHandler = new CustomMvcHandler(new RequestContext(httpContext, routeData));
                mvcHandler.Process(httpContext);
            }
        }

        private class CustomMvcHandler : MvcHandler
        {
            public CustomMvcHandler(RequestContext requestContext)
                : base(requestContext)
            {
            }

            public void Process(HttpContextBase context)
            {
                this.ProcessRequest(context);
            }
        }
    }
}