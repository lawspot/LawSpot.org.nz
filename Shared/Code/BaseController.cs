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

            /// <summary>
            /// Indicates whether the Ask A Lawyer tab is active.
            /// </summary>
            public bool AskALawyerTabActive { get; set; }

            /// <summary>
            /// Indicates whether the Browse Answers tab is active.
            /// </summary>
            public bool BrowseAnswersTabActive { get; set; }

            /// <summary>
            /// Indicates whether the How It Works tab is active.
            /// </summary>
            public bool HowItWorksTabActive { get; set; }
        }

        /// <summary>
        /// Indicates whether the Ask A Lawyer tab is active.
        /// </summary>
        public bool AskALawyerTabActive { get; set; }

        /// <summary>
        /// Indicates whether the Browse Answers tab is active.
        /// </summary>
        public bool BrowseAnswersTabActive { get; set; }

        /// <summary>
        /// Indicates whether the How It Works tab is active.
        /// </summary>
        public bool HowItWorksTabActive { get; set; }

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
                        if (model.User.CanAnswerQuestions)
                            model.SuccessMessage += string.Format(" There are {0} unanswered questions.", GetUnansweredQuestionCount());
                        break;
                    case "loggedout":
                        model.SuccessMessage = "You have logged out.";
                        break;
                    case "registered":
                        model.SuccessMessage = string.Format("Thanks for registering!  Please check your email ({0}) to confirm your account with us.", this.User.EmailAddress);
                        break;
                    case "updated":
                        model.SuccessMessage = "Your changes were saved.";
                        break;
                }

                // Tab activation states.
                model.AskALawyerTabActive = this.AskALawyerTabActive;
                model.BrowseAnswersTabActive = this.BrowseAnswersTabActive;
                model.HowItWorksTabActive = this.HowItWorksTabActive;

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
            var ticket = Lawspot.Shared.CustomPrincipal.FromUser(user, rememberMe).ToTicket(rememberMe);
            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (rememberMe)
                cookie.Expires = DateTime.Now.AddYears(1);  // Remember Me = good for one year.
            this.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Registers a user account and sends the user an email.
        /// </summary>
        /// <param name="emailAddress"> The user's email address. </param>
        /// <param name="password"> The user's password. </param>
        /// <param name="regionId"> The ID of the nearest region. </param>
        /// <returns> A reference to the user. </returns>
        /// <remarks> Call DataContext.SubmitChanges() to save. </remarks>
        protected User Register(string emailAddress, string password, int regionId)
        {
            // Create a random (max 50 char) token.
            var token = new System.Text.StringBuilder();
            var random = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var randomBytes = new byte[50];
            random.GetBytes(randomBytes);
            for (int i = 0; i < 50; i++)
                token.Append((char)((int)'A' + (randomBytes[i] % 26)));

            // Register a new user.
            var user = new User();
            user.CreatedOn = DateTimeOffset.Now;
            user.EmailAddress = emailAddress;
            user.Password = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            user.RegionId = regionId;
            user.EmailValidationToken = token.ToString();
            this.DataContext.Users.InsertOnSubmit(user);

            return user;
        }

        /// <summary>
        /// Sends an email to a newly registered user.
        /// </summary>
        /// <param name="user"> The details of the user that registered. </param>
        /// <param name="password"> The user's password. </param>
        /// <param name="lawyer"> The user registered as a lawyer. </param>
        /// <param name="askedQuestion"> The user registered as part of asking a question. </param>
        /// <returns> A reference to the user. </returns>
        protected void SendRegistrationEmail(User user, string password, bool lawyer, bool askedQuestion)
        {
            // Send them an email.
            var registrationEmail = new Lawspot.Email.RegisterMessage();
            if (lawyer)
                registrationEmail.UseLawyerRegistrationTemplate();
            registrationEmail.To.Add(user.EmailAddress);
            registrationEmail.EmailAddress = user.EmailAddress;
            registrationEmail.Password = password;
            registrationEmail.ValidateEmailUri = string.Format("{0}/validate-email?userId={1}&token={2}",
                registrationEmail.BaseUrl, Uri.EscapeDataString(user.UserId.ToString()),
                Uri.EscapeDataString(user.EmailValidationToken));
            registrationEmail.AskedQuestion = askedQuestion;
            registrationEmail.Send();
        }

        /// <summary>
        /// Gets the total number of unanswered questions.
        /// </summary>
        /// <returns> The total number of unanswered questions. </returns>
        private int GetUnansweredQuestionCount()
        {
            return CacheProvider.CacheDatabaseQuery("UnansweredQuestionCount", connection =>
                connection.Questions.Count(q => q.Answers.Count(a => a.ReviewDate == null || a.Approved) == 0), TimeSpan.FromMinutes(5));
        }
    }
}