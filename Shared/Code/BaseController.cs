﻿using System;
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
        /// Called before an action method is executed.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Call the base class.
            base.OnActionExecuting(filterContext);

            // Check if the cached permissions in the cookie are out of date.
            if (this.User != null && DateTime.Now.Subtract(this.User.LastUpdated).TotalMinutes >= 5)
            {
                this.User.UpdateFromUser(this.UserDetails);
                this.Response.Cookies.Add(this.User.ToCookie(this.User.RememberMe));
            }
        }

        /// <summary>
        /// Gets a reference to the EF database context.
        /// </summary>
        public LawspotDataContext DataContext
        {
            get
            {
                var dataContext = (LawspotDataContext)this.HttpContext.Items["DataContext"];
                if (dataContext == null)
                {
                    dataContext = new LawspotDataContext();
                    dataContext.Log = new DebugTextWriter();
                    this.HttpContext.Items["DataContext"] = dataContext;
                }
                return dataContext;
            }
        }

        private class DebugTextWriter : System.IO.StreamWriter
        {
            public DebugTextWriter()
                : base(new DebugOutStream(), System.Text.Encoding.Unicode, 1024)
            {
                this.AutoFlush = true;
            }

            class DebugOutStream : System.IO.Stream
            {
                public override void Write(byte[] buffer, int offset, int count)
                {
                    System.Diagnostics.Debug.Write(System.Text.Encoding.Unicode.GetString(buffer, offset, count));
                }

                public override bool CanRead { get { return false; } }
                public override bool CanSeek { get { return false; } }
                public override bool CanWrite { get { return true; } }
                public override void Flush() { System.Diagnostics.Debug.Flush(); }
                public override long Length { get { throw new InvalidOperationException(); } }
                public override int Read(byte[] buffer, int offset, int count) { throw new InvalidOperationException(); }
                public override long Seek(long offset, System.IO.SeekOrigin origin) { throw new InvalidOperationException(); }
                public override void SetLength(long value) { throw new InvalidOperationException(); }
                public override long Position
                {
                    get { throw new InvalidOperationException(); }
                    set { throw new InvalidOperationException(); }
                }
            };
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
        /// Complete information about the logged in user.
        /// </summary>
        public User UserDetails
        {
            get
            {
                var user = (User)HttpContext.Items["User"];
                if (user != null)
                    return user;
                user = this.DataContext.Users.Single(u => u.UserId == this.User.Id);
                HttpContext.Items["User"] = user;
                return user;
            }
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

            /// <summary>
            /// A message to contributors (lawyers, question vetters, etc) to help us answer questions
            /// or vet questions or whatever.
            /// </summary>
            public string ContributorMessageHtml { get; set; }

            /// <summary>
            /// Indicates whether or not we are in the admin section.
            /// </summary>
            public bool InAdmin { get; set; }

            /// <summary>
            /// Indicates whether the login session expired.
            /// </summary>
            public bool SessionExpired { get; set; }

            /// <summary>
            /// The current year.
            /// </summary>
            public int CurrentYear { get; set; }
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
        /// The success message to show at the top of the page.
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Indicates whether or not we are in the admin section.
        /// </summary>
        public bool InAdmin { get; set; }

        /// <summary>
        /// Accessing the properties within the model state dictionary would normally give errors
        /// when those properties don't exist, but we can ovveride that behaviour by implementing
        /// IMustacheDataModel.
        /// </summary>
        private class ModelStateDictionary : Dictionary<string, object>, IMustacheDataModel
        {
            /// <summary>
            /// Gets the full name of the type that the properties belong to.
            /// </summary>
            /// <returns> The full name of the type that the properties belong to. </returns>
            public string GetTypeName()
            {
                return typeof(ModelStateDictionary).FullName;
            }

            /// <summary>
            /// Gets the value of the property, if that property exists.
            /// </summary>
            /// <param name="name"> The name of the property. </param>
            /// <param name="value"> Set to the value of the property once the method returns. </param>
            /// <returns> <c>true</c> if the property exists; <c>false</c> otherwise. </returns>
            bool IMustacheDataModel.TryGetValue(string name, out object value)
            {
                base.TryGetValue(name, out value);
                return true;
            }
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
                model.CurrentYear = DateTime.Now.Year;

                // User.
                model.User = ((Controller)viewContext.Controller).User as CustomPrincipal;

                // Translate alert types into messages.
                model.SuccessMessage = this.SuccessMessage;
                switch (viewContext.HttpContext.Request.QueryString["alert"])
                {
                    case "loggedin":
                        model.SuccessMessage = "You're logged in.  Welcome back to LawSpot.";
                        break;
                    case "loggedout":
                        model.SuccessMessage = "You have logged out.";
                        break;
                    case "registered":
                        if (this.User != null)
                            // Normal case.
                            model.SuccessMessage = string.Format("Thanks for registering!  We've sent an email with your login details to {0}", this.User.EmailAddress);
                        else
                            // User logged out on that page, or visited the page via a bookmark.
                            model.SuccessMessage = "Thanks for registering!  We've sent you an email with your login details.";
                        break;
                    case "updated":
                        model.SuccessMessage = "Your changes were saved.";
                        break;
                    case "passwordreset":
                        model.SuccessMessage = "Your password was successfully changed.";
                        break;
                }

                // If a user is logged in and the user is a contributor, show a customized message.
                if (User != null && (User.CanAnswerQuestions || User.CanVetAnswers || User.CanVetLawyers || User.CanVetQuestions))
                    model.ContributorMessageHtml = GetContributorMessageHtml(User);

                // Tab activation states.
                model.AskALawyerTabActive = this.AskALawyerTabActive;
                model.BrowseAnswersTabActive = this.BrowseAnswersTabActive;
                model.HowItWorksTabActive = this.HowItWorksTabActive;

                // Admin section.
                model.InAdmin = this.InAdmin;

                // Show session expired message.
                model.SessionExpired = viewContext.HttpContext.Items.Contains("SessionExpired") ||
                    viewContext.HttpContext.Request.QueryString["sessionExpired"] != null;

                // ModelState.
                var result = new ModelStateDictionary();
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
                                dictionary[keys[i]] = new ModelStateDictionary();
                            dictionary = (ModelStateDictionary)dictionary[keys[i]];
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
            // Update the user login info.
            user.LastLogInDate = DateTimeOffset.Now;
            user.LogInCount = user.LogInCount + 1;
            user.LogInIpAddress = Request.UserHostAddress;
            this.DataContext.SubmitChanges();

            // Send the user an authentication cookie.
            var principal = Lawspot.Shared.CustomPrincipal.FromUser(user, rememberMe);
            this.Response.Cookies.Add(principal.ToCookie(rememberMe));
        }

        /// <summary>
        /// Registers a user account and sends the user an email.
        /// </summary>
        /// <param name="emailAddress"> The user's email address. </param>
        /// <param name="password"> The user's password. </param>
        /// <param name="regionId"> The ID of the nearest region. </param>
        /// <param name="lawyer"> <c>true</c> if the user that is registering is a lawyer; <c>false</c> otherwise. </param>
        /// <returns> A reference to the user. </returns>
        /// <remarks> Call DataContext.SubmitChanges() to save. </remarks>
        protected User Register(string emailAddress, string password, int regionId, bool lawyer = false)
        {
            // Create a new user.
            var user = new User();
            user.CreatedOn = DateTimeOffset.Now;
            user.EmailAddress = emailAddress;
            user.Password = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            user.RegionId = regionId;

            if (lawyer)
            {
                // Create a random (max 50 char) token.
                user.EmailValidated = false;
                user.EmailValidationToken = StringUtilities.CreateRandomToken(50);
            }

            // Save changes.
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
            if (user.EmailValidationToken != null)
            {
                registrationEmail.ValidateEmailUri = string.Format("{0}/validate-email?userId={1}&token={2}",
                    registrationEmail.BaseUrl, Uri.EscapeDataString(user.UserId.ToString()),
                    Uri.EscapeDataString(user.EmailValidationToken));
            }
            registrationEmail.AskedQuestion = askedQuestion;
            registrationEmail.Send();
        }

        private class ActionCount
        {
            public int Count;
            public string Noun;
            public string Uri;
        }

        /// <summary>
        /// Gets the message to display to contributors.
        /// </summary>
        /// <param name="user"> The logged in user. </param>
        /// <returns> A login message. </returns>
        private static string GetContributorMessageHtml(CustomPrincipal user)
        {
            var result = new System.Text.StringBuilder();

            var actions = new List<ActionCount>();
            if (user.CanVetQuestions)
            {
                actions.Add(new ActionCount()
                {
                    Count = CacheProvider.CacheDatabaseQuery("UnreviewedQuestionCount", connection =>
                        connection.Questions.Count(q => q.ReviewDate == null), TimeSpan.FromMinutes(2)),
                    Noun = "unreviewed question",
                    Uri = "/admin/review-questions",
                });
            }
            if (user.CanAnswerQuestions)
            {
                actions.Add(new ActionCount()
                {
                    Count = CacheProvider.CacheDatabaseQuery("UnansweredQuestionCount", connection =>
                        connection.Questions.Count(q => q.Status == QuestionStatus.Approved && q.Answers.Any(a => a.ReviewDate == null || a.Status == AnswerStatus.Approved) == false), TimeSpan.FromMinutes(2)),
                    Noun = "unanswered question",
                    Uri = "/admin/answer-questions",
                });
            }
            if (user.CanVetAnswers)
            {
                actions.Add(new ActionCount()
                {
                    Count = CacheProvider.CacheDatabaseQuery("UnreviewedAnswerCount", connection =>
                        connection.Answers.Count(a => a.ReviewDate == null), TimeSpan.FromMinutes(2)),
                    Noun = "unreviewed answer",
                    Uri = "/admin/review-answers",
                });
            }
            if (user.CanVetLawyers)
            {
                actions.Add(new ActionCount()
                {
                    Count = CacheProvider.CacheDatabaseQuery("UnreviewedLawyerCount", connection =>
                        connection.Users.Count(l => l.ApprovedAsLawyer.HasValue && l.ReviewDate == null && l.EmailValidated == true), TimeSpan.FromMinutes(2)),
                    Noun = "pending lawyer",
                    Uri = "/admin/review-lawyers",
                });
            }
            actions.RemoveAll(c => c.Count == 0);
            if (actions.Count > 0)
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    var action = actions[i];
                    if (i == 0)
                        result.Append(action.Count != 1 ? " There are currently " : " There is currently ");
                    else if (i == actions.Count - 1)
                        result.Append(" and ");
                    else
                        result.Append(", ");
                    result.AppendFormat(@"<a href=""{0}"">", action.Uri);
                    result.Append(action.Count);
                    result.Append(" ");
                    result.Append(action.Noun);
                    if (action.Count != 1)
                        result.Append("s");
                    result.Append("</a>");
                }
                result.Append(".");
            }
            return result.ToString();
        }

        /// <summary>
        /// Saves a new event to the database.
        /// </summary>
        /// <param name="eventType"> The event type. </param>
        /// <param name="userId"> The ID of the user that triggered the event. </param>
        /// <param name="details"> Additional details. </param>
        public void LogEvent(EventType eventType, int userId, object details = null)
        {
            var eventToLog = new Event();
            eventToLog.EventDate = DateTimeOffset.Now;
            eventToLog.EventType = eventType;
            eventToLog.UserId = userId;
            if (details != null)
                eventToLog.Details = Newtonsoft.Json.JsonConvert.SerializeObject(details);
            this.DataContext.Events.InsertOnSubmit(eventToLog);
        }
    }
}