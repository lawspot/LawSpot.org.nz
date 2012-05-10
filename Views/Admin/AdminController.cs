using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.Admin;

namespace Lawspot.Controllers
{
    public class AdminController : BaseController
    {
        /// <summary>
        /// All methods on this controller require that the user be logged in.
        /// </summary>
        /// <param name="filterContext"> The request context. </param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (this.User == null)
                filterContext.Result = new HttpUnauthorizedResult();
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
                return new LayoutViewModel()
                {
                    InAdmin = true,
                };
            }
            return base.GetModel(viewContext, modelType);
        }

        /// <summary>
        /// Returns a response with a status code and a plain text response.
        /// </summary>
        public class StatusPlusTextResult : ActionResult
        {
            public StatusPlusTextResult(int statusCode, string content)
            {
                if (content == null)
                    throw new ArgumentNullException("content");
                this.StatusCode = statusCode;
                this.Content = content;
            }

            /// <summary>
            /// The HTTP status code to return.
            /// </summary>
            public int StatusCode { get; set; }

            /// <summary>
            /// The text to return in the response.
            /// </summary>
            public string Content { get; set; }

            /// <summary>
            /// Enables processing of the result of an action method by a custom type that inherits
            /// from the ActionResult class.
            /// </summary>
            /// <param name="context"> The context in which the result is executed. The context
            /// information includes the controller, HTTP content, request context, and route
            /// data.</param>
            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.Response.StatusCode = this.StatusCode;
                context.HttpContext.Response.ContentType = "text/plain";
                context.HttpContext.Response.Write(this.Content);
            }
        }

        /// <summary>
        /// Displays the answer questions page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AnswerQuestions(string category, string filter, string sort)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                return new HttpStatusCodeResult(403);

            var model = new AnswerQuestionsViewModel();

            // Get the specialisation category of the current user.
            int specialisationCategoryId = 0;
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();
            if (user.IsRegisteredLawyer)
                specialisationCategoryId = user.Lawyer.SpecialisationCategoryId ?? 0;

            // Categories.
            int categoryId = specialisationCategoryId;
            if (category != null)
                categoryId = int.Parse(category);
            model.CategoryOptions = new SelectListItem[] {
                    new SelectListItem()
                    {
                        Text = "All Categories",
                        Value = "0",
                        Selected = categoryId == 0,
                    }
                }.Union(
                    this.DataContext.Categories
                    .OrderBy(c => c.Name)
                    .ToList()
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.CategoryId.ToString(),
                        Selected = c.CategoryId == categoryId,
                    }));

            // Filter.
            var filterValue = AnswerQuestionsFilter.Unanswered;
            if (filter != null)
                filterValue = (AnswerQuestionsFilter)Enum.Parse(typeof(AnswerQuestionsFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "All", Value = AnswerQuestionsFilter.All.ToString(), Selected = filterValue == AnswerQuestionsFilter.All },
                new SelectListItem() { Text = "Unanswered", Value = AnswerQuestionsFilter.Unanswered.ToString(), Selected = filterValue == AnswerQuestionsFilter.Unanswered },
                new SelectListItem() { Text = "Answered", Value = AnswerQuestionsFilter.Answered.ToString(), Selected = filterValue == AnswerQuestionsFilter.Answered },
                new SelectListItem() { Text = "Answered by Me", Value = AnswerQuestionsFilter.AnsweredByMe.ToString(), Selected = filterValue == AnswerQuestionsFilter.AnsweredByMe },
            };

            // Sort order.
            var sortValue = QuestionSortOrder.FirstPosted;
            if (sort != null)
                sortValue = (QuestionSortOrder)Enum.Parse(typeof(QuestionSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First Posted", Value = QuestionSortOrder.FirstPosted.ToString(), Selected = sortValue == QuestionSortOrder.FirstPosted },
                new SelectListItem() { Text = "Most Recent", Value = QuestionSortOrder.MostRecent.ToString(), Selected = sortValue == QuestionSortOrder.MostRecent },
            };

            // Filter and sort the questions.
            IEnumerable<Question> questions = this.DataContext.Questions
                .Where(q => q.Approved == true);
            if (categoryId != 0)
                questions = questions.Where(q => q.CategoryId == categoryId);
            switch (filterValue)
            {
                case AnswerQuestionsFilter.Unanswered:
                    questions = questions.Where(q => q.Answers.Any() == false);
                    break;
                case AnswerQuestionsFilter.Answered:
                    questions = questions.Where(q => q.Answers.Any() == true);
                    break;
                case AnswerQuestionsFilter.AnsweredByMe:
                    questions = questions.Where(q => q.Answers.Any(a => a.CreatedByUserId == this.User.Id));
                    break;
            }
            switch (sortValue)
            {
                case QuestionSortOrder.FirstPosted:
                    questions = questions.OrderBy(q => q.CreatedOn);
                    break;
                case QuestionSortOrder.MostRecent:
                    questions = questions.OrderByDescending(q => q.CreatedOn);
                    break;
            }
            model.Questions = questions
                .Take(100)
                .ToList()
                .Select(q => new QuestionViewModel()
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Details = q.Details,
                    DateAndTime = q.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    CategoryName = q.Category.Name,
                });

            return View(model);
        }

        /// <summary>
        /// Submits an answer to a question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerText"></param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult PostAnswer(int questionId, string answerText)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to answer questions.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(answerText))
                return new StatusPlusTextResult(400, "The answer details cannot be blank.");
            if (answerText.Length > 2000)
                return new StatusPlusTextResult(400, "The answer is too long.");

            // Create a new answer for the question.
            var answer = new Answer();
            answer.CreatedOn = DateTimeOffset.Now;
            answer.Details = answerText;
            answer.CreatedByUserId = this.User.Id;
            answer.QuestionId = questionId;
            this.DataContext.Answers.InsertOnSubmit(answer);
            this.DataContext.SubmitChanges();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Displays the review lawyers page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewLawyers(string category, string filter, string sort)
        {
            // Ensure the user is allow to vet lawyers.
            if (this.User.CanVetLawyers == false)
                return new HttpStatusCodeResult(403);

            var model = new ReviewLawyersViewModel();

            // Populate the list of precanned rejection responses.
            model.CannedRejectionReasons = new SelectListItem[] {
                new SelectListItem() { Text = "Select a canned response", Value = "" },
                new SelectListItem() { Text = "Not Based In Wellington", Value = "You are not based in Wellington: LawSpot is currently being piloted in the Wellington region. This means that for the moment only lawyers based in Wellington will be able to submit answers to questions, and only under the supervision of the Wellington Community Law Centre. We'll be sure to let you know over the next few weeks when lawyers from other regions can start submitting answers. You may still be able to help with the pilot even if you're not based in Wellington - to find out more, please email us at volunteer@lawspot.org.nz." },
                new SelectListItem() { Text = "No Certificate", Value = "You do not appear to hold a current practising certificate." },
            };

            // Categories.
            int categoryId = 0;
            if (category != null)
                categoryId = int.Parse(category);
            model.CategoryOptions = new SelectListItem[] {
                    new SelectListItem()
                    {
                        Text = "All Areas Of Expertise",
                        Value = "0",
                        Selected = categoryId == 0,
                    }
                }.Union(
                    this.DataContext.Categories
                    .OrderBy(c => c.Name)
                    .ToList()
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.CategoryId.ToString(),
                        Selected = c.CategoryId == categoryId,
                    }));

            // Filter.
            var filterValue = LawyerFilter.Unreviewed;
            if (filter != null)
                filterValue = (LawyerFilter)Enum.Parse(typeof(LawyerFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "Unreviewed", Value = LawyerFilter.Unreviewed.ToString(), Selected = filterValue == LawyerFilter.Unreviewed },
                new SelectListItem() { Text = "Approved", Value = LawyerFilter.Approved.ToString(), Selected = filterValue == LawyerFilter.Approved },
                new SelectListItem() { Text = "Rejected", Value = LawyerFilter.Rejected.ToString(), Selected = filterValue == LawyerFilter.Rejected },
            };

            // Sort order.
            var sortValue = LawyerSortOrder.MostRecent;
            if (sort != null)
                sortValue = (LawyerSortOrder)Enum.Parse(typeof(LawyerSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First To Register", Value = LawyerSortOrder.FirstToRegister.ToString(), Selected = sortValue == LawyerSortOrder.FirstToRegister },
                new SelectListItem() { Text = "Most Recent", Value = LawyerSortOrder.MostRecent.ToString(), Selected = sortValue == LawyerSortOrder.MostRecent },
            };

            // Filter and sort.
            IEnumerable<Lawyer> lawyers = this.DataContext.Lawyers;
            if (categoryId != 0)
                lawyers = lawyers.Where(l => l.SpecialisationCategoryId == categoryId);
            switch (filterValue)
            {
                case LawyerFilter.Unreviewed:
                    lawyers = lawyers.Where(l => l.ReviewDate == null);
                    break;
                case LawyerFilter.Approved:
                    lawyers = lawyers.Where(l => l.Approved == true && l.ReviewDate != null);
                    break;
                case LawyerFilter.Rejected:
                    lawyers = lawyers.Where(l => l.Approved == false && l.ReviewDate != null);
                    break;
            }
            switch (sortValue)
            {
                case LawyerSortOrder.FirstToRegister:
                    lawyers = lawyers.OrderBy(l => l.CreatedOn);
                    break;
                case LawyerSortOrder.MostRecent:
                    lawyers = lawyers.OrderByDescending(l => l.CreatedOn);
                    break;
            }
            model.Lawyers = lawyers
                .Take(100)
                .ToList()
                .Select(l => new LawyerViewModel()
                {
                    LawyerId = l.LawyerId,
                    Approved = l.Approved,
                    NameHtml = TrimWithTooltip(l.FullName, 20),
                    EmailAddressHtml = TrimWithTooltip(l.User.EmailAddress, 25),
                    DateRegistered = l.CreatedOn.ToString("d MMM yyyy"),
                    YearAdmitted = l.YearOfAdmission,
                });

            return View(model);
        }

        /// <summary>
        /// Creates a text span, truncated and with a tooltip if the text is too long.
        /// </summary>
        /// <param name="text"> The text to display. </param>
        /// <param name="maxLength"> The maximum length of the text. </param>
        /// <returns> A HTML string to display the text. </returns>
        private string TrimWithTooltip(string text, int maxLength)
        {
            if (text == null)
                return string.Empty;
            if (text.Length <= maxLength)
                return System.Net.WebUtility.HtmlEncode(text);
            return string.Format(@"<span title=""{0}"">{1}</span>", System.Net.WebUtility.HtmlEncode(text),
                System.Net.WebUtility.HtmlEncode(text.Substring(0, maxLength) + "..."));
        }

        /// <summary>
        /// Approves a lawyer.
        /// </summary>
        /// <param name="questionId"> The ID of the lawyer to approve. </param>
        /// <param name="title"> The new title of the question. </param>
        /// <param name="details"> The new question details. </param>
        /// <param name="categoryId"> The new category ID. </param>
        [HttpPost]
        public StatusPlusTextResult ApproveLawyer(int lawyerId)
        {
            // Ensure the user is allow to vet lawyers.
            if (this.User.CanVetLawyers == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to approve lawyers.");

            var lawyer = this.DataContext.Lawyers.Where(l => l.LawyerId == lawyerId).SingleOrDefault();
            if (lawyer == null)
                return new StatusPlusTextResult(400, "The lawyer account doesn't exist.");
            lawyer.Approved = true;
            lawyer.ReviewDate = DateTimeOffset.Now;
            lawyer.ReviewedByUserId = this.User.Id;
            lawyer.RejectionReason = null;
            lawyer.User.CanAnswerQuestions = true;
            this.DataContext.SubmitChanges();

            // Send a message to the lawyer saying that their account has approved.
            var acceptanceMessage = new Email.LawyerApprovedMessage();
            acceptanceMessage.To.Add(lawyer.User.EmailDisplayName);
            acceptanceMessage.Name = lawyer.User.EmailGreeting;
            acceptanceMessage.Send();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Rejects a lawyer.
        /// </summary>
        /// <param name="questionId"> The ID of the lawyer to reject. </param>
        /// <param name="reason"> The reason for rejecting the lawyer. </param>
        [HttpPost]
        public StatusPlusTextResult RejectLawyer(int lawyerId, string reason)
        {
            // Ensure the user is allow to vet lawyers.
            if (this.User.CanVetLawyers == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to reject lawyers.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(reason))
                return new StatusPlusTextResult(400, "Please enter a reason why the lawyer account is being rejected.");
            if (reason.Length > 1000)
                return new StatusPlusTextResult(400, "Your rejection reason is too long.");

            var lawyer = this.DataContext.Lawyers.Where(l => l.LawyerId == lawyerId).SingleOrDefault();
            if (lawyer == null)
                return new StatusPlusTextResult(400, "The lawyer account doesn't exist.");
            lawyer.Approved = false;
            lawyer.ReviewDate = DateTimeOffset.Now;
            lawyer.ReviewedByUserId = this.User.Id;
            lawyer.RejectionReason = reason;
            lawyer.User.CanAnswerQuestions = false;
            this.DataContext.SubmitChanges();

            // Send a message to the lawyer saying that their account has been "put on hold".
            var rejectionMessage = new Email.LawyerRejectedMessage();
            rejectionMessage.To.Add(lawyer.User.EmailDisplayName);
            rejectionMessage.Name = lawyer.User.EmailGreeting;
            rejectionMessage.Reason = reason;
            rejectionMessage.Send();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Displays the review questions page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewQuestions(string category, string filter, string sort)
        {
            // Ensure the user is allow to vet questions.
            if (this.User.CanVetQuestions == false)
                return new HttpStatusCodeResult(403);

            var model = new ReviewQuestionsViewModel();

            // Populate the list of precanned rejection responses.
            model.CannedRejectionReasons = new SelectListItem[] {
                new SelectListItem() { Text = "Select a canned response", Value = "" },
                new SelectListItem() { Text = "Duplicate Question", Value = "Your question, or a similar question, has already been answered and published by LawSpot." },
                new SelectListItem() { Text = "Off-topic", Value = "Your question relates to an area of law that LawSpot doesn’t cover: LawSpot does not answer questions about conveyancing or property leasing (except residential tenancies), or questions from landlords (unless they are community groups), or from business ventures and commercial employers." },
                new SelectListItem() { Text = "Offensive Material", Value = "Your question contained offensive material." },
                new SelectListItem() { Text = "Privacy Issue", Value = "Your question contained information that may, if published online, reveal the identity of a particular person or organisation." },
            };

            // Categories.
            int categoryId = 0;
            if (category != null)
                categoryId = int.Parse(category);
            model.CategoryOptions = new SelectListItem[] {
                    new SelectListItem()
                    {
                        Text = "All Categories",
                        Value = "0",
                        Selected = categoryId == 0,
                    }
                }.Union(this.DataContext.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.CategoryId.ToString(),
                        Selected = c.CategoryId == categoryId,
                    }));

            // Filter.
            var filterValue = ReviewQuestionsFilter.Unreviewed;
            if (filter != null)
                filterValue = (ReviewQuestionsFilter)Enum.Parse(typeof(ReviewQuestionsFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "Unreviewed", Value = ReviewQuestionsFilter.Unreviewed.ToString(), Selected = filterValue == ReviewQuestionsFilter.Unreviewed },
                new SelectListItem() { Text = "Approved", Value = ReviewQuestionsFilter.Approved.ToString(), Selected = filterValue == ReviewQuestionsFilter.Approved },
                new SelectListItem() { Text = "Rejected", Value = ReviewQuestionsFilter.Rejected.ToString(), Selected = filterValue == ReviewQuestionsFilter.Rejected },
            };

            // Sort order.
            var sortValue = QuestionSortOrder.FirstPosted;
            if (sort != null)
                sortValue = (QuestionSortOrder)Enum.Parse(typeof(QuestionSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First Posted", Value = QuestionSortOrder.FirstPosted.ToString(), Selected = sortValue == QuestionSortOrder.FirstPosted },
                new SelectListItem() { Text = "Most Recent", Value = QuestionSortOrder.MostRecent.ToString(), Selected = sortValue == QuestionSortOrder.MostRecent },
            };

            // Filter and sort the questions.
            IEnumerable<Question> questions = this.DataContext.Questions;
            if (categoryId != 0)
                questions = questions.Where(q => q.CategoryId == categoryId);
            switch (filterValue)
            {
                case ReviewQuestionsFilter.Unreviewed:
                    questions = questions.Where(q => q.ReviewDate == null && q.User.EmailValidated);
                    break;
                case ReviewQuestionsFilter.Approved:
                    questions = questions.Where(q => q.ReviewDate != null && q.Approved == true);
                    break;
                case ReviewQuestionsFilter.Rejected:
                    questions = questions.Where(q => q.ReviewDate != null && q.Approved == false);
                    break;
            }
            switch (sortValue)
            {
                case QuestionSortOrder.FirstPosted:
                    questions = questions.OrderBy(q => q.CreatedOn);
                    break;
                case QuestionSortOrder.MostRecent:
                    questions = questions.OrderByDescending(q => q.CreatedOn);
                    break;
            }
            model.Questions = questions
                .ToList()
                .Select(q => new QuestionViewModel()
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Details = q.Details,
                    DateAndTime = q.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                });

            // Categories only (used inside form).
            model.Categories =
                this.DataContext.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.CategoryId.ToString(),
                    });

            return View(model);
        }

        /// <summary>
        /// Approves a question and optionally modifies it.
        /// </summary>
        /// <param name="questionId"> The ID of the question to approve. </param>
        /// <param name="title"> The new title of the question. </param>
        /// <param name="details"> The new question details. </param>
        /// <param name="categoryId"> The new category ID. </param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult ApproveQuestion(int questionId, string title, string details, int categoryId)
        {
            // Ensure the user is allow to vet questions.
            if (this.User.CanVetQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to approve questions.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(title))
                return new StatusPlusTextResult(400, "The question title cannot be blank.");
            if (title.Length > 150)
                return new StatusPlusTextResult(400, "The question title is too long.");
            if (string.IsNullOrWhiteSpace(details))
                return new StatusPlusTextResult(400, "The question details cannot be blank.");
            if (details.Length > 600)
                return new StatusPlusTextResult(400, "The question details are too long.");

            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).SingleOrDefault();
            if (question == null)
                return new StatusPlusTextResult(400, "The question doesn't exist.");
            question.Title = title;
            question.Details = details;
            question.CategoryId = categoryId;
            question.Approved = true;
            question.ReviewDate = DateTimeOffset.Now;
            question.ReviewedByUserId = this.User.Id;
            question.RejectionReason = null;
            this.DataContext.SubmitChanges();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Rejects a question.
        /// </summary>
        /// <param name="questionId"> The ID of the question to reject. </param>
        /// <param name="reason"> The reason for rejecting the question. </param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult RejectQuestion(int questionId, string reason)
        {
            // Ensure the user is allow to vet questions.
            if (this.User.CanVetQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to reject questions.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(reason))
                return new StatusPlusTextResult(400, "Please enter a reason why the question is being rejected.");
            if (reason.Length > 1000)
                return new StatusPlusTextResult(400, "Your rejection reason is too long.");

            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).SingleOrDefault();
            if (question == null)
                return new StatusPlusTextResult(400, "The question doesn't exist.");
            question.Approved = false;
            question.ReviewDate = DateTimeOffset.Now;
            question.ReviewedByUserId = this.User.Id;
            question.RejectionReason = reason;
            this.DataContext.SubmitChanges();

            // Send a message to the user saying their question has been rejected.
            var rejectionMessage = new Email.QuestionRejectedMessage();
            rejectionMessage.To.Add(question.User.EmailDisplayName);
            rejectionMessage.Question = question.Title;
            rejectionMessage.QuestionDate = question.CreatedOn.ToString("d MMM");
            rejectionMessage.Reason = reason;
            rejectionMessage.Send();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Displays the review answers page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewAnswers(string category, string filter, string sort)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                return new HttpStatusCodeResult(403);

            var model = new ReviewAnswersViewModel();

            // Populate the list of precanned rejection responses.
            model.CannedRejectionReasons = new SelectListItem[] {
                new SelectListItem() { Text = "Select a canned response", Value = "" },
                new SelectListItem() { Text = "Didn't Answer Question", Value = "You haven’t answered the question - try again." },
                new SelectListItem() { Text = "Too Complex", Value = "Your answer is too long/poorly drafted/too complex for users - try again." },
                new SelectListItem() { Text = "Bzzz Wrong", Value = "You’ve got the law wrong on this – try again." },
                new SelectListItem() { Text = "Law Change", Value = "The law in this area has recently changed – try again." },
            };

            // Categories.
            int categoryId = 0;
            if (category != null)
                categoryId = int.Parse(category);
            model.CategoryOptions = new SelectListItem[] {
                    new SelectListItem()
                    {
                        Text = "All Categories",
                        Value = "0",
                        Selected = categoryId == 0,
                    }
                }.Union(this.DataContext.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.CategoryId.ToString(),
                        Selected = c.CategoryId == categoryId,
                    }));

            // Filter.
            var filterValue = ReviewAnswersFilter.Unreviewed;
            if (filter != null)
                filterValue = (ReviewAnswersFilter)Enum.Parse(typeof(ReviewAnswersFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "Unreviewed", Value = ReviewAnswersFilter.Unreviewed.ToString(), Selected = filterValue == ReviewAnswersFilter.Unreviewed },
                new SelectListItem() { Text = "Approved", Value = ReviewAnswersFilter.Approved.ToString(), Selected = filterValue == ReviewAnswersFilter.Approved },
                new SelectListItem() { Text = "Rejected", Value = ReviewAnswersFilter.Rejected.ToString(), Selected = filterValue == ReviewAnswersFilter.Rejected },
            };

            // Sort order.
            var sortValue = AnswerSortOrder.FirstPosted;
            if (sort != null)
                sortValue = (AnswerSortOrder)Enum.Parse(typeof(AnswerSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First Posted", Value = AnswerSortOrder.FirstPosted.ToString(), Selected = sortValue == AnswerSortOrder.FirstPosted },
                new SelectListItem() { Text = "Most Recent", Value = AnswerSortOrder.MostRecent.ToString(), Selected = sortValue == AnswerSortOrder.MostRecent },
            };

            // Filter and sort the answers.
            IEnumerable<Answer> answers = this.DataContext.Answers;
            if (categoryId != 0)
                answers = answers.Where(a => a.Question.CategoryId == categoryId);
            switch (filterValue)
            {
                case ReviewAnswersFilter.Unreviewed:
                    answers = answers.Where(a => a.ReviewDate == null);
                    break;
                case ReviewAnswersFilter.Approved:
                    answers = answers.Where(a => a.Approved == true && a.ReviewDate != null);
                    break;
                case ReviewAnswersFilter.Rejected:
                    answers = answers.Where(a => a.Approved == false && a.ReviewDate != null);
                    break;
            }
            switch (sortValue)
            {
                case AnswerSortOrder.FirstPosted:
                    answers = answers.OrderBy(q => q.CreatedOn);
                    break;
                case AnswerSortOrder.MostRecent:
                    answers = answers.OrderByDescending(q => q.CreatedOn);
                    break;
            }
            model.Answers = answers
                .ToList()
                .Select(a => new AnswerViewModel()
                {
                    AnswerId = a.AnswerId,
                    Title = a.Question.Title,
                    Details = a.Question.Details,
                    CategoryName = a.Question.Category.Name,
                    DateAndTime = a.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    Answer = a.Details,
                });

            return View(model);
        }

        /// <summary>
        /// Approves an answer and optionally modifies it.
        /// </summary>
        /// <param name="answerId"> The ID of the answer to approve. </param>
        /// <param name="answerDetails"> The new answer details. </param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult ApproveAnswer(int answerId, string answerDetails)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to approve answers.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(answerDetails))
                return new StatusPlusTextResult(400, "The answer details cannot be blank.");
            if (answerDetails.Length > 2000)
                return new StatusPlusTextResult(400, "The answer is too long.");

            // Approve the anwer in the DB.
            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).SingleOrDefault();
            if (answer == null)
                return new StatusPlusTextResult(400, "The answer doesn't exist.");
            answer.Details = answerDetails;
            answer.Approved = true;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            answer.RejectionReason = null;
            this.DataContext.SubmitChanges();

            // Send a message to the lawyer that answered the question.
            var answerPublishedMessage = new Email.AnswerApprovedMessage();
            answerPublishedMessage.To.Add(answer.User.EmailDisplayName);
            answerPublishedMessage.Name = answer.User.EmailGreeting;
            answerPublishedMessage.Question = answer.Question.Title;
            answerPublishedMessage.QuestionUrl = answerPublishedMessage.BaseUrl + answer.Question.AbsolutePath;
            answerPublishedMessage.Answer = answer.Details;
            answerPublishedMessage.UnansweredQuestionCount = 0;
            answerPublishedMessage.Send();

            // Send a message to the user who asked the question.
            var questionAnsweredMessage = new Email.QuestionAnsweredMessage();
            questionAnsweredMessage.To.Add(answer.Question.User.EmailAddress);
            questionAnsweredMessage.Question = answer.Question.Title;
            questionAnsweredMessage.Answer = answer.Details;
            questionAnsweredMessage.Send();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Rejects an answer.
        /// </summary>
        /// <param name="answerId"> The ID of the answer to reject. </param>
        /// <param name="reason"> The reason for rejecting the answer. </param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult RejectAnswer(int answerId, string reason)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to reject answers.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(reason))
                return new StatusPlusTextResult(400, "Please enter a reason why the answer is being rejected.");
            if (reason.Length > 1000)
                return new StatusPlusTextResult(400, "Your rejection reason is too long.");

            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).SingleOrDefault();
            if (answer == null)
                return new StatusPlusTextResult(400, "The answer doesn't exist.");
            answer.Approved = false;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            answer.RejectionReason = reason;
            this.DataContext.SubmitChanges();

            // Send a message to the lawyer saying their answer has been rejected.
            var rejectionMessage = new Email.AnswerRejectedMessage();
            rejectionMessage.To.Add(answer.User.EmailDisplayName);
            rejectionMessage.Name = answer.User.EmailGreeting;
            rejectionMessage.Question = answer.Question.Title;
            rejectionMessage.AnswerDate = answer.CreatedOn.ToString("d MMM");
            rejectionMessage.Reason = reason;
            rejectionMessage.Send();

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Displays the account settings page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AccountSettings()
        {
            return View(InitializeAccountSettingsViewModel());
        }

        /// <summary>
        /// Populates a AccountSettingsViewModel object.
        /// </summary>
        /// <returns></returns>
        private AccountSettingsViewModel InitializeAccountSettingsViewModel()
        {
            // Get the user details.
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();

            var model = new AccountSettingsViewModel();
            if (this.Request.Form["EmailAddress"] == null)
                model.EmailAddress = user.EmailAddress;
            if (this.Request.Form["RegionId"] == null)
                model.RegionId = user.RegionId;
            model.RegionName = user.Region.Name;
            model.Regions = this.DataContext.Regions
                .Select(r => new SelectListItem()
                {
                    Text = r.Name,
                    Value = r.RegionId.ToString(),
                    Selected = r.RegionId == user.RegionId,
                });
            return model;
        }

        /// <summary>
        /// When applied to an action method, calls the method when a parameter with the given key
        /// matches the given value.
        /// </summary>
        private class FormSelectorAttribute : ActionMethodSelectorAttribute
        {
            public FormSelectorAttribute(string key, string value)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                if (value == null)
                    throw new ArgumentNullException("value");
                this.Key = key;
                this.Value = value;
            }

            /// <summary>
            /// The form key that must be present to select the method.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// The value that the form key must have in order to select the method.
            /// </summary>
            public string Value { get; set; }

            /// <summary>
            /// Determines whether the action method selection is valid for the specified controller context.
            /// </summary>
            /// <param name="controllerContext"> The controller context. </param>
            /// <param name="methodInfo"> Information about the action method. </param>
            /// <returns> <c>true</c> if the action method selection is valid for the specified
            /// controller context; otherwise, <c>false</c>. </returns>
            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
            {
                var formValue = controllerContext.HttpContext.Request.Form[this.Key];
                if (formValue == this.Value)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Called to change the user's email address.
        /// </summary>
        /// <returns></returns>
        [HttpPost, ActionName("AccountSettings"), FormSelector("selector", "email")]
        public ActionResult ChangeEmailAddress(ChangeEmailAddressViewModel model)
        {
            // Check that there isn't already a user with that email address.
            var existingUser = this.DataContext.Users.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);
            if (existingUser != null)
                ModelState.AddModelError("EmailAddress", "That email address is already in use by another member.");

            if (ModelState.IsValid == false)
            {
                var model2 = InitializeAccountSettingsViewModel();
                model2.EmailAddress = model.EmailAddress;
                model2.ExpandEmailAddressSection = true;
                return View(model2);
            }

            // Change the user's email address.
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();
            user.EmailAddress = model.EmailAddress;
            this.DataContext.SubmitChanges();

            // Re-issue the login cookie with the new email address.
            Login(user, this.User.RememberMe);

            return RedirectToAction("AccountSettings", new { alert = "updated" });
        }

        /// <summary>
        /// Called to change the user's region.
        /// </summary>
        /// <returns></returns>
        [HttpPost, ActionName("AccountSettings"), FormSelector("selector", "region")]
        public ActionResult ChangeRegion(int regionId)
        {
            // Change the user's region.
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();
            user.RegionId = regionId;
            this.DataContext.SubmitChanges();

            return RedirectToAction("AccountSettings", new { alert = "updated" });
        }
    }
}
