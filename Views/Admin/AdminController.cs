using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Shared;
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
        /// Called before an action method executes.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            // Don't show the volunteer action message.
            this.InAdmin = true;
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
        /// Displays the activity stream page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ActivityStream()
        {
            var model = new ActivityStreamViewModel();

            // Question stats.
            model.QuestionsSubmitted = this.DataContext.Questions
                .Count(q => q.CreatedByUserId == this.User.Id);
            model.QuestionsPublished = this.DataContext.Questions
                .Count(q => q.CreatedByUserId == this.User.Id && q.Approved);
            var lastQuestionSubmitted = this.DataContext.Questions
                .Where(q => q.CreatedByUserId == this.User.Id)
                .OrderByDescending(q => q.CreatedOn).FirstOrDefault();
            model.LastQuestionSubmitted = lastQuestionSubmitted == null ? "N/A" :
                StringUtilities.ConvertToRelativeTime(DateTimeOffset.Now.Subtract(lastQuestionSubmitted.CreatedOn));

            // Question vetter stats.
            if (this.User.CanVetQuestions)
            {
                model.QuestionsReviewed = this.DataContext.Questions
                    .Count(q => q.ReviewedByUserId == this.User.Id);
                var lastQuestionReviewed = this.DataContext.Questions
                    .Where(q => q.ReviewedByUserId == this.User.Id)
                    .OrderByDescending(q => q.ReviewDate)
                    .FirstOrDefault();
                model.LastQuestionReviewed = lastQuestionReviewed == null ? "N/A" :
                    StringUtilities.ConvertToRelativeTime(DateTimeOffset.Now.Subtract(lastQuestionReviewed.CreatedOn));
            }

            // Answer stats.
            if (this.User.CanAnswerQuestions)
            {
                model.AnswersSubmitted = this.DataContext.Answers
                    .Count(a => a.CreatedByUserId == this.User.Id);
                model.AnswersPublished = this.DataContext.Answers
                    .Count(a => a.CreatedByUserId == this.User.Id && a.Approved);
                var lastAnswerSubmitted = this.DataContext.Answers
                    .Where(a => a.CreatedByUserId == this.User.Id)
                    .OrderByDescending(a => a.CreatedOn)
                    .FirstOrDefault();
                model.LastAnswerSubmitted = lastAnswerSubmitted == null ? "N/A" :
                    StringUtilities.ConvertToRelativeTime(DateTimeOffset.Now.Subtract(lastAnswerSubmitted.CreatedOn));
            }

            // Answer vetter stats.
            if (this.User.CanVetAnswers)
            {
                model.AnswersReviewed = this.DataContext.Answers
                    .Count(a => a.ReviewedByUserId == this.User.Id);
                var lastAnswerReviewed = this.DataContext.Answers
                    .Where(a => a.ReviewedByUserId == this.User.Id)
                    .OrderByDescending(a => a.ReviewDate)
                    .FirstOrDefault();
                model.LastAnswerReviewed = lastAnswerReviewed == null ? "N/A" :
                    StringUtilities.ConvertToRelativeTime(DateTimeOffset.Now.Subtract(lastAnswerReviewed.CreatedOn));
            }

            return View(model);
        }

        private enum AnswerOrDraftStatus
        {
            Draft,
            Pending,
            Approved,
            Rejected,
        }

        private class AnswerOrDraft
        {
            public int QuestionId { get; set; }
            public string Details { get; set; }
            public string References { get; set; }
            public DateTimeOffset UpdatedOn { get; set; }
            public User User { get; set; }
            public AnswerOrDraftStatus Status { get; set; }

            public string Notification
            {
                get
                {
                    switch (this.Status)
                    {
                        case AnswerOrDraftStatus.Draft:
                            return string.Format("{0} has started drafting an answer for this question (last updated: {1:g})",
                                this.User.DisplayName, this.UpdatedOn);
                        case AnswerOrDraftStatus.Pending:
                            return string.Format("{0} posted an answer for this question on {1:g}",
                                this.User.DisplayName, this.UpdatedOn);
                        case AnswerOrDraftStatus.Approved:
                            return string.Format("{0} posted an answer for this question and it was approved.",
                                this.User.DisplayName, this.UpdatedOn);
                        case AnswerOrDraftStatus.Rejected:
                            return string.Format("{0} posted an answer for this question but it was rejected on {1:g}",
                                this.User.DisplayName, this.UpdatedOn);
                    }
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Displays the answer questions page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AnswerQuestions(string category, string filter, int? questionId, string sort, int page = 1)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                throw new HttpException(403, "Access denied");

            var model = new AnswerQuestionsViewModel();

            // Get the specialisation category of the current user.
            int specialisationCategoryId = 0;
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();
            if (user.IsRegisteredLawyer)
                specialisationCategoryId = user.Lawyer.SpecialisationCategoryId ?? 0;

            // Categories.
            int categoryId = questionId.HasValue ? 0 : specialisationCategoryId;
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
            var filterValue = questionId.HasValue ? AnswerQuestionsFilter.SingleQuestion : AnswerQuestionsFilter.Unanswered;
            if (filter != null)
                filterValue = (AnswerQuestionsFilter)Enum.Parse(typeof(AnswerQuestionsFilter), filter, true);
            if (filterValue == AnswerQuestionsFilter.SingleQuestion && questionId.HasValue == false)
                filterValue = AnswerQuestionsFilter.Unanswered;
            var filterOptions = new List<SelectListItem>
            {
                new SelectListItem() { Text = "All", Value = AnswerQuestionsFilter.All.ToString(), Selected = filterValue == AnswerQuestionsFilter.All },
                new SelectListItem() { Text = "Unanswered", Value = AnswerQuestionsFilter.Unanswered.ToString(), Selected = filterValue == AnswerQuestionsFilter.Unanswered },
                new SelectListItem() { Text = "Pending Approval", Value = AnswerQuestionsFilter.Pending.ToString(), Selected = filterValue == AnswerQuestionsFilter.Pending },
                new SelectListItem() { Text = "Answered", Value = AnswerQuestionsFilter.Answered.ToString(), Selected = filterValue == AnswerQuestionsFilter.Answered },
                new SelectListItem() { Text = "Answered by Me", Value = AnswerQuestionsFilter.AnsweredByMe.ToString(), Selected = filterValue == AnswerQuestionsFilter.AnsweredByMe },
            };
            if (filterValue == AnswerQuestionsFilter.SingleQuestion)
                filterOptions.Add(new SelectListItem() { Text = "Single Question", Value = AnswerQuestionsFilter.SingleQuestion.ToString(), Selected = true });
            model.FilterOptions = filterOptions;

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
                    questions = questions.Where(q => q.Answers.Any(a => a.Approved == true || a.ReviewDate == null) == false);
                    break;
                case AnswerQuestionsFilter.Pending:
                    questions = questions.Where(q => q.Answers.Where(a => a.Approved == false).Any(a => a.ReviewDate == null) == true);
                    break;
                case AnswerQuestionsFilter.Answered:
                    questions = questions.Where(q => q.Answers.Any(a => a.Approved == true) == true);
                    break;
                case AnswerQuestionsFilter.AnsweredByMe:
                    questions = questions.Where(q => q.Answers.Any(a => a.CreatedByUserId == this.User.Id));
                    break;
                case AnswerQuestionsFilter.SingleQuestion:
                    questions = questions.Where(q => q.QuestionId == questionId.Value);
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
            model.Questions = new PagedListView<AnswerQuestionViewModel>(questions
                .Select(q => new AnswerQuestionViewModel()
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    DetailsHtml = StringUtilities.ConvertTextToHtml(q.Details),
                    DateAndTime = q.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    CategoryName = q.Category.Name,
                    Answer = string.Empty,
                    References = string.Empty,
                }), page, 10, Request.Url);

            // Get all the draft answers and answers relating to the questions.
            var answers = GetAnswers(model.Questions.Items.Select(q => q.QuestionId));

            // Populate the questions with draft answer details.
            foreach (var question in model.Questions.Items)
            {
                var ownAnswer = answers.Where(a => a.QuestionId == question.QuestionId &&
                    a.User.UserId == this.User.Id).OrderByDescending(a => a.UpdatedOn).FirstOrDefault();
                if (ownAnswer != null)
                {
                    // The user has a answer for this question.
                    question.Answer = ownAnswer.Details;
                    question.References = ownAnswer.References;
                }

                var otherAnswer = answers.Where(a => a.QuestionId == question.QuestionId &&
                    a.User.UserId != this.User.Id).OrderByDescending(a => a.UpdatedOn).FirstOrDefault();
                if (otherAnswer != null)
                {
                    // Another user has a draft for this question.
                    question.Notification = otherAnswer.Notification;
                }
            }

            return View(model);
        }

        /// <summary>
        /// Submits an answer to a question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerText"></param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult PostAnswer(int questionId, string answerText, string references)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to answer questions.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(answerText))
                return new StatusPlusTextResult(400, "The answer details cannot be blank.");
            if (answerText.Length > 20000)
                return new StatusPlusTextResult(400, "The answer is too long.");
            if (references.Length > 20000)
                return new StatusPlusTextResult(400, "The references are too long.");

            // Create a new answer for the question.
            var answer = new Answer();
            answer.CreatedOn = DateTimeOffset.Now;
            answer.Details = answerText;
            answer.CreatedByUserId = this.User.Id;
            answer.QuestionId = questionId;
            answer.References = references;
            this.DataContext.Answers.InsertOnSubmit(answer);

            // Delete any drafts.
            this.DataContext.DraftAnswers.DeleteAllOnSubmit(this.DataContext.DraftAnswers.Where(da =>
                da.CreatedByUserId == this.User.Id && da.QuestionId == questionId));

            // Save changes.
            this.DataContext.SubmitChanges();

            return new StatusPlusTextResult(200, StringUtilities.ConvertTextToHtml(answer.Details));
        }

        /// <summary>
        /// Checks the status of a question.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult CheckQuestionStatus(int questionId)
        {
            var answers = GetAnswers(new int[] { questionId });
            var existingAnswer = answers.Where(a => a.User.UserId != this.User.Id)
                .OrderByDescending(a => a.UpdatedOn).FirstOrDefault();
            if (existingAnswer != null)
                return new StatusPlusTextResult(200, existingAnswer.Notification);
            return new StatusPlusTextResult(200, string.Empty);
        }

        /// <summary>
        /// Gets a list of answers for the given question IDs, either draft or posted.
        /// </summary>
        /// <param name="questionIds"></param>
        /// <returns></returns>
        private List<AnswerOrDraft> GetAnswers(IEnumerable<int> questionIds)
        {
            var answers = questionIds.Join(this.DataContext.DraftAnswers,
                qId => qId,
                da => da.QuestionId,
                (q, da) => new AnswerOrDraft()
                {
                    QuestionId = da.QuestionId,
                    Details = da.Details,
                    References = da.References,
                    UpdatedOn = da.UpdatedOn,
                    User = da.User,
                    Status = AnswerOrDraftStatus.Draft,
                });
            answers = answers.Union(questionIds.Join(this.DataContext.Answers,
                qId => qId,
                da => da.QuestionId,
                (q, a) => new AnswerOrDraft()
                {
                    QuestionId = a.QuestionId,
                    Details = a.Details,
                    References = a.References,
                    UpdatedOn = a.CreatedOn,
                    User = a.User,
                    Status = a.ReviewDate != null && a.Approved ? AnswerOrDraftStatus.Approved :
                        (a.ReviewDate != null && a.Approved == false ? AnswerOrDraftStatus.Rejected : AnswerOrDraftStatus.Pending),
                }));
            return answers.ToList();
        }

        /// <summary>
        /// Saves an answer as a draft.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="answerText"></param>
        /// <param name="references"></param>
        /// <returns></returns>
        [HttpPost]
        public StatusPlusTextResult SaveDraftAnswer(int questionId, string answerText, string references)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to answer questions.");

            // Check if there is already a draft.
            var draftAnswer = this.DataContext.DraftAnswers.Where(da => da.CreatedByUserId == this.User.Id &&
                da.QuestionId == questionId).SingleOrDefault();

            // If there is nothing to save, delete the draft.
            if (string.IsNullOrWhiteSpace(answerText) && string.IsNullOrWhiteSpace(references))
            {
                if (draftAnswer != null)
                {
                    this.DataContext.DraftAnswers.DeleteOnSubmit(draftAnswer);
                    this.DataContext.SubmitChanges();
                }
            }
            else
            {

                if (draftAnswer == null)
                {
                    // Create a new draft answer.
                    draftAnswer = new DraftAnswer();
                    this.DataContext.DraftAnswers.InsertOnSubmit(draftAnswer);
                    draftAnswer.CreatedOn = DateTimeOffset.Now;
                    draftAnswer.CreatedByUserId = this.User.Id;
                    draftAnswer.QuestionId = questionId;
                }

                // Save the changes to the draft answer.
                draftAnswer.Details = answerText ?? string.Empty;
                draftAnswer.References = references ?? string.Empty;
                draftAnswer.UpdatedOn = DateTimeOffset.Now;
                this.DataContext.SubmitChanges();

            }

            return CheckQuestionStatus(questionId);
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
                throw new HttpException(403, "Access denied");

            var model = new ReviewLawyersViewModel();

            // Populate the list of precanned rejection responses.
            model.CannedRejectionReasons = new SelectListItem[] {
                new SelectListItem() { Text = "Select a canned response", Value = "" },
                new SelectListItem() { Text = "Not Based In Wellington", Value = "You are not based in Wellington: LawSpot is currently being piloted in the Wellington region. This means that for the moment only lawyers based in Wellington will be able to submit answers to questions, and only under the supervision of Community Law Wellington & Hutt Valley. We'll be sure to let you know over the next few weeks when lawyers from other regions can start submitting answers. You may still be able to help with the pilot even if you're not based in Wellington - to find out more, please email us at volunteer@lawspot.org.nz." },
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
            IEnumerable<Lawyer> lawyers = this.DataContext.Lawyers
                .Where(l => l.User.EmailValidated == true);
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
            bool statusChange = lawyer.Approved != true || lawyer.ReviewDate == null;
            lawyer.Approved = true;
            lawyer.ReviewDate = DateTimeOffset.Now;
            lawyer.ReviewedByUserId = this.User.Id;
            lawyer.RejectionReason = null;
            lawyer.User.CanAnswerQuestions = true;
            this.DataContext.SubmitChanges();

            // Send a message to the lawyer saying that their account has approved.
            // But only if the lawyer's status has changed.
            if (statusChange)
            {
                var acceptanceMessage = new Email.LawyerApprovedMessage();
                acceptanceMessage.To.Add(lawyer.User.EmailDisplayName);
                acceptanceMessage.Name = lawyer.User.EmailGreeting;
                acceptanceMessage.Send();
            }

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
            bool statusChange = lawyer.Approved != false || lawyer.ReviewDate == null;
            lawyer.Approved = false;
            lawyer.ReviewDate = DateTimeOffset.Now;
            lawyer.ReviewedByUserId = this.User.Id;
            lawyer.RejectionReason = reason;
            lawyer.User.CanAnswerQuestions = false;
            this.DataContext.SubmitChanges();

            // Send a message to the lawyer saying that their account has been "put on hold".
            // But only if the lawyer's status has changed.
            if (statusChange)
            {
                var rejectionMessage = new Email.LawyerRejectedMessage();
                rejectionMessage.To.Add(lawyer.User.EmailDisplayName);
                rejectionMessage.Name = lawyer.User.EmailGreeting;
                rejectionMessage.Reason = reason;
                rejectionMessage.Send();
            }

            return new StatusPlusTextResult(200, "Success");
        }

        /// <summary>
        /// Displays the review questions page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewQuestions(string category, string filter, string sort, int page = 1)
        {
            // Ensure the user is allow to vet questions.
            if (this.User.CanVetQuestions == false)
                throw new HttpException(403, "Access denied");

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
                new SelectListItem() { Text = "Approved By Me", Value = ReviewQuestionsFilter.ApprovedByMe.ToString(), Selected = filterValue == ReviewQuestionsFilter.ApprovedByMe },
                new SelectListItem() { Text = "Rejected", Value = ReviewQuestionsFilter.Rejected.ToString(), Selected = filterValue == ReviewQuestionsFilter.Rejected },
                new SelectListItem() { Text = "Rejected By Me", Value = ReviewQuestionsFilter.RejectedByMe.ToString(), Selected = filterValue == ReviewQuestionsFilter.RejectedByMe },
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
                    questions = questions.Where(q => q.ReviewDate == null);
                    break;
                case ReviewQuestionsFilter.Approved:
                    questions = questions.Where(q => q.ReviewDate != null && q.Approved == true);
                    break;
                case ReviewQuestionsFilter.ApprovedByMe:
                    questions = questions.Where(q => q.ReviewDate != null && q.Approved == true && q.ReviewedByUserId == this.User.Id);
                    break;
                case ReviewQuestionsFilter.Rejected:
                    questions = questions.Where(q => q.ReviewDate != null && q.Approved == false);
                    break;
                case ReviewQuestionsFilter.RejectedByMe:
                    questions = questions.Where(q => q.ReviewDate != null && q.Approved == false && q.ReviewedByUserId == this.User.Id);
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
            model.Questions = new PagedListView<ReviewQuestionViewModel>(questions
                .ToList()
                .Select(q => new ReviewQuestionViewModel()
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Details = q.Details,
                    DateAndTime = q.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    CategoryId = q.CategoryId,
                    CategoryName = q.Category.Name,
                }), page, 10, Request.Url);

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

            // Update the search index.
            SearchIndexer.UpdateQuestion(question);

            // Recalculate the number of approved questions in the category.
            this.DataContext.ExecuteCommand(@"
                UPDATE Category
                SET ApprovedQuestionCount = (
                    SELECT COUNT(*)
                    FROM Question
                    WHERE Question.CategoryId = Category.CategoryId
                        AND Approved = 1)
                WHERE Category.CategoryId = {0}", question.CategoryId);

            return new StatusPlusTextResult(200, StringUtilities.ConvertTextToHtml(question.Details));
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
            bool statusChange = question.Approved != false || question.ReviewDate == null;
            question.Approved = false;
            question.ReviewDate = DateTimeOffset.Now;
            question.ReviewedByUserId = this.User.Id;
            question.RejectionReason = reason;
            this.DataContext.SubmitChanges();

            // Send a message to the user saying their question has been rejected.
            // But only if the question's status has changed.
            if (statusChange)
            {
                var rejectionMessage = new Email.QuestionRejectedMessage();
                rejectionMessage.To.Add(question.User.EmailDisplayName);
                rejectionMessage.Question = question.Title;
                rejectionMessage.QuestionDate = question.CreatedOn.ToString("d MMM");
                rejectionMessage.ReasonHtml = StringUtilities.ConvertTextToHtml(reason);
                rejectionMessage.Send();
            }

            // Update the search index.
            SearchIndexer.UpdateQuestion(question);

            // Recalculate the number of approved questions in the category.
            this.DataContext.ExecuteCommand(@"
                UPDATE Category
                SET ApprovedQuestionCount = (
                    SELECT COUNT(*)
                    FROM Question
                    WHERE Question.CategoryId = Category.CategoryId
                        AND Approved = 1)
                WHERE Category.CategoryId = {0}", question.CategoryId);

            return new StatusPlusTextResult(200, StringUtilities.ConvertTextToHtml(reason));
        }

        private void UpdateCategoryQuestionCounts()
        {
            
        }

        /// <summary>
        /// Displays the review answers page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewAnswers(string category, string filter, string sort, int page = 1)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                throw new HttpException(403, "Access denied");

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
                new SelectListItem() { Text = "Approved By Me", Value = ReviewAnswersFilter.ApprovedByMe.ToString(), Selected = filterValue == ReviewAnswersFilter.ApprovedByMe },
                new SelectListItem() { Text = "Rejected", Value = ReviewAnswersFilter.Rejected.ToString(), Selected = filterValue == ReviewAnswersFilter.Rejected },
                new SelectListItem() { Text = "Rejected By Me", Value = ReviewAnswersFilter.RejectedByMe.ToString(), Selected = filterValue == ReviewAnswersFilter.RejectedByMe },
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
                case ReviewAnswersFilter.ApprovedByMe:
                    answers = answers.Where(a => a.Approved == true && a.ReviewDate != null && a.ReviewedByUserId == this.User.Id);
                    break;
                case ReviewAnswersFilter.Rejected:
                    answers = answers.Where(a => a.Approved == false && a.ReviewDate != null);
                    break;
                case ReviewAnswersFilter.RejectedByMe:
                    answers = answers.Where(a => a.Approved == false && a.ReviewDate != null && a.ReviewedByUserId == this.User.Id);
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
            model.Answers = new PagedListView<AnswerViewModel>(answers
                .ToList()
                .Select(a => new AnswerViewModel()
                {
                    AnswerId = a.AnswerId,
                    Title = a.Question.Title,
                    Details = a.Question.Details,
                    CategoryName = a.Question.Category.Name,
                    DateAndTime = a.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    Answer = a.Details,
                    AnsweredBy = a.User.EmailDisplayName,
                    ReferencesHtml = StringUtilities.ConvertTextToHtml(a.References),
                }), page, 10, Request.Url);

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
            if (answerDetails.Length > 20000)
                return new StatusPlusTextResult(400, "The answer is too long.");

            // Approve the anwer in the DB.
            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).SingleOrDefault();
            if (answer == null)
                return new StatusPlusTextResult(400, "The answer doesn't exist.");
            bool statusChange = answer.Approved != true || answer.ReviewDate == null;
            answer.Details = answerDetails;
            answer.Approved = true;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            answer.RejectionReason = null;
            this.DataContext.SubmitChanges();

            // Only send emails if the answer's status has changed.
            if (statusChange)
            {

                // Send a message to the lawyer that answered the question.
                var answerPublishedMessage = new Email.AnswerApprovedMessage();
                answerPublishedMessage.To.Add(answer.User.EmailDisplayName);
                answerPublishedMessage.Name = answer.User.EmailGreeting;
                answerPublishedMessage.Question = answer.Question.Title;
                answerPublishedMessage.QuestionUrl = answerPublishedMessage.BaseUrl + answer.Question.AbsolutePath;
                answerPublishedMessage.Answer = answer.Details;
                answerPublishedMessage.UnansweredQuestionCount = this.DataContext.Questions.
                    Count(q => q.Approved == true && q.Answers.Count(a => a.Approved == true) == 0);
                answerPublishedMessage.Send();

                // Send a message to the user who asked the question.
                var questionAnsweredMessage = new Email.QuestionAnsweredMessage();
                questionAnsweredMessage.To.Add(answer.Question.User.EmailAddress);
                questionAnsweredMessage.Question = answer.Question.Title;
                questionAnsweredMessage.Answer = answer.Details;
                questionAnsweredMessage.Send();

            }

            // Update the search index.
            SearchIndexer.UpdateQuestion(answer.Question);

            return new StatusPlusTextResult(200, StringUtilities.ConvertTextToHtml(answer.Details));
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
            bool statusChange = answer.Approved != false || answer.ReviewDate == null;
            answer.Approved = false;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            answer.RejectionReason = reason;
            this.DataContext.SubmitChanges();

            // Only send an email if the answer's status has changed.
            if (statusChange)
            {
                // Send a message to the lawyer saying their answer has been rejected.
                var rejectionMessage = new Email.AnswerRejectedMessage();
                rejectionMessage.To.Add(answer.User.EmailDisplayName);
                rejectionMessage.Name = answer.User.EmailGreeting;
                rejectionMessage.Question = answer.Question.Title;
                rejectionMessage.AnswerDate = answer.CreatedOn.ToString("d MMM");
                rejectionMessage.ReasonHtml = StringUtilities.ConvertTextToHtml(reason);
                rejectionMessage.Send();
            }

            // Update the search index.
            SearchIndexer.UpdateQuestion(answer.Question);

            return new StatusPlusTextResult(200, StringUtilities.ConvertTextToHtml(reason));
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
            if (existingUser != null && existingUser.UserId != this.User.Id)
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
        /// Called to change the user's password.
        /// </summary>
        /// <returns></returns>
        [HttpPost, ActionName("AccountSettings"), FormSelector("selector", "password")]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                var model2 = InitializeAccountSettingsViewModel();
                model2.ExpandPasswordSection = true;
                return View(model2);
            }

            // Change the user's password.
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 12);
            this.DataContext.SubmitChanges();

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

        /// <summary>
        /// Displays the vetter policy page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult VetterPolicy()
        {
            // Ensure the user is allowed to vet questions.
            if (this.User.CanVetQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");
            return View();
        }

        /// <summary>
        /// Displays the admin page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Admin(int? sent)
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");
            
            // Show a custom message after sending reminder emails.
            if (sent.HasValue)
                this.SuccessMessage = string.Format("Sent reminder email to {0} lawyers.", sent);

            return View();
        }

        /// <summary>
        /// Called to send reminder emails to lawyers.
        /// </summary>
        /// <returns></returns>
        [HttpPost, ActionName("Admin"), FormSelector("SendReminderEmails", "")]
        public ActionResult SendReminderEmails()
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");

            // Get a list of all the questions that have no answers or even draft answers.
            var questions = this.DataContext.Questions.Where(q => q.Approved && q.Answers.Any() == false && q.DraftAnswers.Any() == false).ToList();

            int sentMessageCount = 0;
            if (questions.Count > 0)
            {
                // Compose messages to all the lawyers.
                var messages = new List<Email.LawyerReminderMessage>();
                foreach (var lawyer in this.DataContext.Lawyers)
                    messages.Add(new Email.LawyerReminderMessage(lawyer, questions));

                // Send the messages.
                foreach (var message in messages)
                    message.Send();

                sentMessageCount = messages.Count;
            }

            return RedirectToAction("Admin", new { sent = sentMessageCount.ToString() });
        }

        /// <summary>
        /// Called to rebuild the search index.
        /// </summary>
        /// <returns></returns>
        [HttpPost, ActionName("Admin"), FormSelector("RebuildSearchIndex", "")]
        public ActionResult RebuildSearchIndex()
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");
            SearchIndexer.RebuildIndex();
            return RedirectToAction("Admin", new { alert = "updated" });
        }
    }
}
