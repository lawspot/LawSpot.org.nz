using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Shared;
using Lawspot.Views.Admin;
using AQ1 = Lawspot.Views.Admin.AnswerQuestions;
using AQ2 = Lawspot.Views.Admin.AnswerQuestion;

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

            // Recent drafts
            model.RecentDrafts = this.DataContext.DraftAnswers
                .Where(da => da.CreatedByUserId == this.User.Id)
                .OrderByDescending(da => da.UpdatedOn)
                .Take(10)
                .ToList()
                .Select(da => new ActivityStreamRecentDraft()
                {
                    Title = da.Question.Title,
                    LastModified = da.UpdatedOn.ToString("d MMM yyyy h:mmtt"),
                    QuestionId = da.QuestionId,
                }).ToList();

            // Recent answers.
            model.RecentAnswers = this.DataContext.Answers
                .Where(a => a.CreatedByUserId == this.User.Id)
                .OrderByDescending(a => a.CreatedOn)
                .Take(10)
                .ToList()
                .Select(a => new ActivityStreamRecentAnswer()
                {
                    Title = a.Question.Title,
                    SubmitDate = a.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    Status = a.Status.ToString(),
                    QuestionId = a.QuestionId,
                }).ToList();

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
                    .Count(a => a.CreatedByUserId == this.User.Id && a.Status == AnswerStatus.Approved);
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

        /// <summary>
        /// Displays the answer questions page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="questionId"></param>
        /// <param name="sort"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="overrideCategory"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AnswerQuestions(string category, string filter, int? questionId, string sort, string search, int page = 1, bool overrideCategory = false)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                throw new HttpException(403, "Access denied");

            var model = new AQ1.AnswerQuestionsViewModel();

            // Get the specialisation category of the current user.
            int specialisationCategoryId = 0;
            var user = this.DataContext.Users.Where(u => u.UserId == this.User.Id).Single();
            if (user.IsRegisteredLawyer)
                specialisationCategoryId = user.Lawyer.SpecialisationCategoryId ?? 0;

            // Categories.
            int categoryId = questionId.HasValue || overrideCategory ? 0 : specialisationCategoryId;
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
            var filterValue = questionId.HasValue ? AQ1.AnswerQuestionsFilter.SingleQuestion : AQ1.AnswerQuestionsFilter.Unanswered;
            if (filter != null)
                filterValue = (AQ1.AnswerQuestionsFilter)Enum.Parse(typeof(AQ1.AnswerQuestionsFilter), filter, true);
            if (filterValue == AQ1.AnswerQuestionsFilter.SingleQuestion && questionId.HasValue == false)
                filterValue = AQ1.AnswerQuestionsFilter.Unanswered;
            var filterOptions = new List<SelectListItem>
            {
                new SelectListItem() { Text = "All", Value = AQ1.AnswerQuestionsFilter.All.ToString(), Selected = filterValue == AQ1.AnswerQuestionsFilter.All },
                new SelectListItem() { Text = "Unanswered", Value = AQ1.AnswerQuestionsFilter.Unanswered.ToString(), Selected = filterValue == AQ1.AnswerQuestionsFilter.Unanswered },
                new SelectListItem() { Text = "Pending Approval", Value = AQ1.AnswerQuestionsFilter.Pending.ToString(), Selected = filterValue == AQ1.AnswerQuestionsFilter.Pending },
                new SelectListItem() { Text = "Answered", Value = AQ1.AnswerQuestionsFilter.Answered.ToString(), Selected = filterValue == AQ1.AnswerQuestionsFilter.Answered },
                new SelectListItem() { Text = "Answered by Me", Value = AQ1.AnswerQuestionsFilter.AnsweredByMe.ToString(), Selected = filterValue == AQ1.AnswerQuestionsFilter.AnsweredByMe },
            };
            if (filterValue == AQ1.AnswerQuestionsFilter.SingleQuestion)
                filterOptions.Add(new SelectListItem() { Text = "Single Question", Value = AQ1.AnswerQuestionsFilter.SingleQuestion.ToString(), Selected = true });
            model.FilterOptions = filterOptions;

            // Sort order.
            var sortValue = AQ1.QuestionSortOrder.MostRecent;
            if (sort != null)
                sortValue = (AQ1.QuestionSortOrder)Enum.Parse(typeof(AQ1.QuestionSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First Posted", Value = AQ1.QuestionSortOrder.FirstPosted.ToString(), Selected = sortValue == AQ1.QuestionSortOrder.FirstPosted },
                new SelectListItem() { Text = "Most Recent", Value = AQ1.QuestionSortOrder.MostRecent.ToString(), Selected = sortValue == AQ1.QuestionSortOrder.MostRecent },
            };

            // Filter and sort the questions.
            IEnumerable<Question> questions = this.DataContext.Questions
                .Where(q => q.Approved == true);
            if (categoryId != 0)
                questions = questions.Where(q => q.CategoryId == categoryId);
            switch (filterValue)
            {
                case AQ1.AnswerQuestionsFilter.Unanswered:
                    questions = questions.Where(q => q.Answers.All(a => a.Status == AnswerStatus.Rejected));
                    break;
                case AQ1.AnswerQuestionsFilter.Pending:
                    questions = questions.Where(q => q.Answers.Any(a => a.ReviewDate == null || a.Status == AnswerStatus.Recommended) == true &&
                        q.Answers.Any(a => a.Status == AnswerStatus.Approved) == false);
                    break;
                case AQ1.AnswerQuestionsFilter.Answered:
                    questions = questions.Where(q => q.Answers.Any(a => a.Status == AnswerStatus.Approved) == true);
                    break;
                case AQ1.AnswerQuestionsFilter.AnsweredByMe:
                    questions = questions.Where(q => q.Answers.Any(a => a.CreatedByUserId == this.User.Id));
                    break;
                case AQ1.AnswerQuestionsFilter.SingleQuestion:
                    questions = questions.Where(q => q.QuestionId == questionId.Value);
                    break;
            }
            switch (sortValue)
            {
                case AQ1.QuestionSortOrder.FirstPosted:
                    questions = questions.OrderBy(q => q.CreatedOn);
                    break;
                case AQ1.QuestionSortOrder.MostRecent:
                    questions = questions.OrderByDescending(q => q.CreatedOn);
                    break;
            }
            if (string.IsNullOrWhiteSpace(search) == false)
            {
                model.Search = search;
                var searchHits = SearchIndexer.Search(search, publicOnly: false);
                questions = questions.Join(searchHits, q => q.QuestionId, sh => sh.ID, (q, sh) => q);
            }
            model.Questions = new PagedListView<AQ1.AnswerQuestionViewModel>(questions
                .Select(q => new AQ1.AnswerQuestionViewModel()
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    DetailsHtml = StringUtilities.ConvertTextToHtml(q.Details),
                    ReviewedBy = this.User.CanAdminister || this.User.CanVetAnswers ? q.ReviewedByUser.EmailDisplayName : null,
                    DateAndTime = q.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    CategoryName = q.Category.Name,
                }), page, 10, Request.Url);

            // If this is the landing page for a lawyer AND there are no questions in the lawyer's category,
            // then show all questions instead.
            if (model.Questions.TotalCount == 0 && Request.QueryString.Count == 0 && overrideCategory == false)
                return AnswerQuestions(category, filter, questionId, sort, null, page, overrideCategory: true);

            // Get answers and draft answers for all the questions.
            var questionIds = model.Questions.Items.Select(q => q.QuestionId).ToArray();
            var answers = this.DataContext.Answers
                .Where(a => questionIds.Contains(a.QuestionId))
                .Select(a => new
                {
                    QuestionId = a.QuestionId,
                    DraftOrRejected = a.Status == AnswerStatus.Rejected,
                    UserId = a.CreatedByUserId,
                    Date = a.ReviewDate.HasValue ? a.ReviewDate.Value : a.CreatedOn
                }).Union(this.DataContext.DraftAnswers
                .Where(a => questionIds.Contains(a.QuestionId))
                .Select(da => new
                {
                    QuestionId = da.QuestionId,
                    DraftOrRejected = true,
                    UserId = da.CreatedByUserId,
                    Date = da.UpdatedOn
                })).ToList();
            foreach (var question in model.Questions.Items)
            {
                // There is:
                // a) A draft by another member that was updated within the last two days.
                // b) A rejected answer by another member that was reviewed within the last two days.
                // c) An answer that is not rejected that was created by another member.
                question.AnsweredByAnother = answers.Any(a => a.QuestionId == question.QuestionId &&
                    a.UserId != this.User.Id &&
                    ((a.DraftOrRejected && DateTimeOffset.Now.Subtract(a.Date).TotalDays < 2.0) || !a.DraftOrRejected));

                // The current user has posted an answer to the question.
                question.AnsweredByMe = answers.Any(a => a.QuestionId == question.QuestionId &&
                    a.UserId == this.User.Id);
            }

            return View(model);
        }

        /// <summary>
        /// Displays the answer question page.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult AnswerQuestion(int questionId)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                throw new HttpException(403, "Access denied");

            var question = DataContext.Questions.Single(q => q.QuestionId == questionId);
            var model = new AQ2.AnswerQuestionViewModel()
            {
                QuestionId = questionId,
                Title = question.Title,
                DetailsHtml = StringUtilities.ConvertTextToHtml(question.Details),
                DateAndTime = question.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                CategoryName = question.Category.Name,
                //OriginalTitle = question.OriginalTitle,
                //OriginalDetailsHtml = StringUtilities.ConvertTextToHtml(question.OriginalDetails),
                ReviewedBy = question.ReviewedByUser != null ? question.ReviewedByUser.EmailDisplayName : null,
                ReviewDate = question.ReviewDate.HasValue ? question.ReviewDate.Value.ToString("d MMM yyyy h:mmtt") : string.Empty,
            };
            model.Answers = question.Answers
                .Select(a => new AQ2.AnswerViewModel()
                {
                    DateAndTime = a.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    AnswerHtml = StringUtilities.ConvertTextToHtml(a.Details),
                    AnsweredBy = a.CreatedByUser.EmailDisplayName,
                    ReferencesHtml = StringUtilities.ConvertTextToHtml(a.References),
                    ReviewedBy = a.ReviewedByUser != null ? a.ReviewedByUser.EmailDisplayName : null,
                    ReviewDate = a.ReviewDate.HasValue ? a.ReviewDate.Value.ToString("d MMM yyyy h:mmtt") : string.Empty,
                    Approved = a.Status == AnswerStatus.Approved,
                    Rejected = a.Status == AnswerStatus.Rejected,
                    Pending = a.ReviewedByUser == null,
                    RecommendedForApproval = a.Status == AnswerStatus.Recommended,
                    RejectionReasonHtml = StringUtilities.ConvertTextToHtml(a.RejectionReason),
                    Draft = false,
                    SortKey = a.ReviewDate.HasValue ? a.ReviewDate.Value : a.CreatedOn,
                }).ToList();

            // Insert other people's drafts.
            model.Answers = model.Answers.Union(this.DataContext.DraftAnswers
                .Where(da => da.QuestionId == questionId)
                .Where(da => da.CreatedByUserId != this.User.Id)
                .ToList()
                .Select(da => new AQ2.AnswerViewModel()
                {
                    DateAndTime = da.UpdatedOn.ToString("d MMM yyyy h:mmtt"),
                    AnswerHtml = StringUtilities.ConvertTextToHtml(da.Details),
                    AnsweredBy = da.CreatedByUser.EmailDisplayName,
                    ReferencesHtml = StringUtilities.ConvertTextToHtml(da.References),
                    Draft = true,
                    SortKey = da.UpdatedOn,
                })).OrderBy(a => a.SortKey).ToList();

            // If we have our own draft answer, show that.
            // Note: there should only be one, but just in case...
            var myDraft = this.DataContext.DraftAnswers
                .Where(da => da.CreatedByUserId == this.User.Id && da.QuestionId == questionId)
                .OrderByDescending(da => da.UpdatedOn)
                .FirstOrDefault();
            if (myDraft != null)
            {
                model.Answer = myDraft.Details;
                model.References = myDraft.References;
            }

            return View(model);
        }

        /// <summary>
        /// Called when an answer is submitted.
        /// </summary>
        /// <param name="questionId"></param>
        /// <param name="details"></param>
        /// <param name="references"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AnswerQuestion(int questionId, string details, string references)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to answer questions.");

            // Validate the input.
            if (string.IsNullOrWhiteSpace(details))
                ModelState.AddModelError("Answer", "The answer details cannot be blank.");
            if (details.Length > 20000)
                ModelState.AddModelError("Answer", "The answer is too long.");
            if (references.Length > 20000)
                ModelState.AddModelError("References", "The references are too long.");

            // Check the model is valid.
            if (ModelState.IsValid == false)
            {
                var model = (AQ2.AnswerQuestionViewModel)((ViewResult)AnswerQuestion(questionId)).Model;
                model.Answer = details;
                model.References = references;
                return View(model);
            }

            // Create a new answer for the question.
            var answer = new Answer();
            answer.CreatedOn = DateTimeOffset.Now;
            answer.Details = details;
            answer.CreatedByUserId = this.User.Id;
            answer.QuestionId = questionId;
            answer.References = references;
            this.DataContext.Answers.InsertOnSubmit(answer);

            // Delete any drafts.
            this.DataContext.DraftAnswers.DeleteAllOnSubmit(this.DataContext.DraftAnswers.Where(da =>
                da.CreatedByUserId == this.User.Id && da.QuestionId == questionId));

            // Log an event.
            LogEvent(EventType.CreateAnswer, this.User.Id, new { QuestionId = questionId, Details = details, References = references });

            // Save changes.
            this.DataContext.SubmitChanges();

            // Update the search index.
            SearchIndexer.UpdateQuestion(answer.Question);

            return RedirectToAction("AnswerQuestion", new { questionId = questionId, alert = "updated" });
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

                return new StatusPlusTextResult(200, "Draft was deleted.");
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

                return new StatusPlusTextResult(200, "Draft was saved.");
            }
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
            if (reason.Length > 20000)
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
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewQuestions(string category, string filter, string sort, string search, int page = 1, int? questionId = null)
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
            var filterValue = questionId.HasValue ? ReviewQuestionsFilter.All : ReviewQuestionsFilter.Unreviewed;
            if (filter != null)
                filterValue = (ReviewQuestionsFilter)Enum.Parse(typeof(ReviewQuestionsFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "All", Value = ReviewQuestionsFilter.All.ToString(), Selected = filterValue == ReviewQuestionsFilter.All },
                new SelectListItem() { Text = "Unreviewed", Value = ReviewQuestionsFilter.Unreviewed.ToString(), Selected = filterValue == ReviewQuestionsFilter.Unreviewed },
                new SelectListItem() { Text = "Approved", Value = ReviewQuestionsFilter.Approved.ToString(), Selected = filterValue == ReviewQuestionsFilter.Approved },
                new SelectListItem() { Text = "Approved By Me", Value = ReviewQuestionsFilter.ApprovedByMe.ToString(), Selected = filterValue == ReviewQuestionsFilter.ApprovedByMe },
                new SelectListItem() { Text = "Rejected", Value = ReviewQuestionsFilter.Rejected.ToString(), Selected = filterValue == ReviewQuestionsFilter.Rejected },
                new SelectListItem() { Text = "Rejected By Me", Value = ReviewQuestionsFilter.RejectedByMe.ToString(), Selected = filterValue == ReviewQuestionsFilter.RejectedByMe },
            };

            // Sort order.
            var sortValue = AQ1.QuestionSortOrder.MostRecent;
            if (sort != null)
                sortValue = (AQ1.QuestionSortOrder)Enum.Parse(typeof(AQ1.QuestionSortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First Posted", Value = AQ1.QuestionSortOrder.FirstPosted.ToString(), Selected = sortValue == AQ1.QuestionSortOrder.FirstPosted },
                new SelectListItem() { Text = "Most Recent", Value = AQ1.QuestionSortOrder.MostRecent.ToString(), Selected = sortValue == AQ1.QuestionSortOrder.MostRecent },
            };

            // Filter and sort the questions.
            IEnumerable<Question> questions = this.DataContext.Questions;
            if (categoryId != 0)
                questions = questions.Where(q => q.CategoryId == categoryId);
            if (questionId.HasValue)
                questions = questions.Where(q => q.QuestionId == questionId);
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
                case AQ1.QuestionSortOrder.FirstPosted:
                    questions = questions.OrderBy(q => q.CreatedOn);
                    break;
                case AQ1.QuestionSortOrder.MostRecent:
                    questions = questions.OrderByDescending(q => q.CreatedOn);
                    break;
            }
            if (string.IsNullOrWhiteSpace(search) == false)
            {
                model.Search = search;
                var searchHits = SearchIndexer.Search(search, publicOnly: false);
                questions = questions.Join(searchHits, q => q.QuestionId, sh => sh.ID, (q, sh) => q);
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
                    ReviewedBy = this.User.CanAdminister && q.ReviewedByUser != null ? q.ReviewedByUser.EmailDisplayName : null,
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
            if (question.Title != title && question.OriginalTitle == null)
                question.OriginalTitle = question.Title;
            question.Title = title;
            if (question.Details != details && question.OriginalDetails == null)
                question.OriginalDetails = question.Details;
            question.Details = details;
            question.CategoryId = categoryId;
            question.Approved = true;
            question.ReviewDate = DateTimeOffset.Now;
            question.ReviewedByUserId = this.User.Id;
            question.RejectionReason = null;
            this.DataContext.SubmitChanges();

            // Log an event.
            LogEvent(EventType.ApproveQuestion, this.User.Id, new { QuestionId = questionId, Title = title, Details = details, CategoryId = categoryId });
            this.DataContext.SubmitChanges();

            // Update the search index.
            SearchIndexer.UpdateQuestion(question);

            // Recalculate the number of approved questions in the category.
            UpdateCategory(question.CategoryId);

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
            if (reason.Length > 20000)
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

            // Log an event.
            LogEvent(EventType.RejectQuestion, this.User.Id, new { QuestionId = questionId, Reason = reason });
            this.DataContext.SubmitChanges();

            // Send a message to the user saying their question has been rejected.
            // But only if the question's status has changed.
            if (statusChange)
            {
                var rejectionMessage = new Email.QuestionRejectedMessage();
                rejectionMessage.To.Add(question.CreatedByUser.EmailDisplayName);
                rejectionMessage.Question = question.Title;
                rejectionMessage.QuestionDate = question.CreatedOn.ToString("d MMM");
                rejectionMessage.ReasonHtml = StringUtilities.ConvertTextToHtml(reason);
                rejectionMessage.Send();
            }

            // Update the search index.
            SearchIndexer.UpdateQuestion(question);

            // Recalculate the number of approved questions in the category.
            UpdateCategory(question.CategoryId);

            return new StatusPlusTextResult(200, StringUtilities.ConvertTextToHtml(reason));
        }

        /// <summary>
        /// Displays the review answers page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewAnswers(string category, string filter, string sort, string search, int page = 1)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                throw new HttpException(403, "Access denied");

            var model = new ReviewAnswersViewModel();

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
                new SelectListItem() { Text = "All", Value = ReviewAnswersFilter.All.ToString(), Selected = filterValue == ReviewAnswersFilter.All },
                new SelectListItem() { Text = "Unreviewed", Value = ReviewAnswersFilter.Unreviewed.ToString(), Selected = filterValue == ReviewAnswersFilter.Unreviewed },
                new SelectListItem() { Text = "Approved", Value = ReviewAnswersFilter.Approved.ToString(), Selected = filterValue == ReviewAnswersFilter.Approved },
                new SelectListItem() { Text = "Approved by Me", Value = ReviewAnswersFilter.ApprovedByMe.ToString(), Selected = filterValue == ReviewAnswersFilter.ApprovedByMe },
                new SelectListItem() { Text = "Rejected", Value = ReviewAnswersFilter.Rejected.ToString(), Selected = filterValue == ReviewAnswersFilter.Rejected },
                new SelectListItem() { Text = "Rejected by Me", Value = ReviewAnswersFilter.RejectedByMe.ToString(), Selected = filterValue == ReviewAnswersFilter.RejectedByMe },
                new SelectListItem() { Text = "Recommended for Approval", Value = ReviewAnswersFilter.RecommendedForApproval.ToString(), Selected = filterValue == ReviewAnswersFilter.RecommendedForApproval },
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
                    if (this.UserDetails.PublisherId.HasValue)
                        answers = answers.Where(a => a.Status == AnswerStatus.Unreviewed || a.Status == AnswerStatus.Recommended);
                    else
                        answers = answers.Where(a => a.Status == AnswerStatus.Unreviewed);
                    break;
                case ReviewAnswersFilter.Approved:
                    if (this.UserDetails.PublisherId.HasValue)
                        answers = answers.Where(a => a.Status == AnswerStatus.Approved);
                    else
                        answers = answers.Where(a => a.Status == AnswerStatus.Approved || a.Status == AnswerStatus.Recommended);
                    break;
                case ReviewAnswersFilter.ApprovedByMe:
                    answers = answers.Where(a => (a.Status == AnswerStatus.Approved || a.Status == AnswerStatus.Recommended) && a.ReviewedByUserId == this.User.Id);
                    break;
                case ReviewAnswersFilter.Rejected:
                    answers = answers.Where(a => a.Status == AnswerStatus.Rejected);
                    break;
                case ReviewAnswersFilter.RejectedByMe:
                    answers = answers.Where(a => a.Status == AnswerStatus.Rejected && a.ReviewedByUserId == this.User.Id);
                    break;
                case ReviewAnswersFilter.RecommendedForApproval:
                    answers = answers.Where(a => a.Status == AnswerStatus.Recommended);
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
            if (string.IsNullOrWhiteSpace(search) == false)
            {
                model.Search = search;
                var searchHits = SearchIndexer.Search(search, publicOnly: false);
                answers = answers.Join(searchHits, a => a.QuestionId, sh => sh.ID, (a, sh) => a);
            }
            model.Answers = new PagedListView<QuestionAndAnswerViewModel>(answers
                .ToList()
                .Select(a => new QuestionAndAnswerViewModel()
                {
                    QuestionId = a.QuestionId,
                    AnswerId = a.AnswerId,
                    Title = a.Question.Title,
                    CategoryName = a.Question.Category.Name,
                    DateAndTime = a.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    IconFileName = a.Status == AnswerStatus.Recommended ? "answer-icon-flagged.png" : "answer-icon.png",
                }), page, 10, Request.Url);

            return View(model);
        }

        /// <summary>
        /// Displays the review answer page.
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewAnswer(int questionId)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                throw new HttpException(403, "Access denied");

            var question = DataContext.Questions.Single(q => q.QuestionId == questionId);
            var model = new ReviewAnswerViewModel()
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                DetailsHtml = StringUtilities.ConvertTextToHtml(question.Details),
                DateAndTime = question.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                CategoryName = question.Category.Name,
                ReviewedBy = question.ReviewedByUser != null ? question.ReviewedByUser.EmailDisplayName : null,
                ReviewDate = question.ReviewDate.HasValue ? question.ReviewDate.Value.ToString("d MMM yyyy h:mmtt") : string.Empty,
            };
            model.Answers = question.Answers
                .Select(a => new AnswerViewModel()
                {
                    AnswerId = a.AnswerId,
                    DateAndTime = a.CreatedOn.ToString("d MMM yyyy h:mmtt"),
                    Answer = a.Details,
                    AnswerHtml = StringUtilities.ConvertTextToHtml(a.Details),
                    AnsweredBy = a.CreatedByUser.EmailDisplayName,
                    ReferencesHtml = StringUtilities.ConvertTextToHtml(a.References),
                    ReviewedBy = a.ReviewedByUser != null ? a.ReviewedByUser.EmailDisplayName : null,
                    ReviewDate = a.ReviewDate.HasValue ? a.ReviewDate.Value.ToString("d MMM yyyy h:mmtt") : string.Empty,
                    Approved = a.Status == AnswerStatus.Approved,
                    Rejected = a.Status == AnswerStatus.Rejected,
                    RecommendedForApproval = a.Status == AnswerStatus.Recommended,
                    CanApproveOrReject = a.Status == AnswerStatus.Unreviewed || a.Status == AnswerStatus.Recommended,
                    RejectionReasonHtml = StringUtilities.ConvertTextToHtml(a.RejectionReason),
                    CannedRejectionReasons = new SelectListItem[] {
                        new SelectListItem() { Text = "Select a canned response", Value = "" },
                        new SelectListItem() { Text = "Didn't Answer Question", Value = "You haven’t answered the question - try again." },
                        new SelectListItem() { Text = "Too Complex", Value = "Your answer is too long/poorly drafted/too complex for users - try again." },
                        new SelectListItem() { Text = "Bzzz Wrong", Value = "You’ve got the law wrong on this – try again." },
                        new SelectListItem() { Text = "Law Change", Value = "The law in this area has recently changed – try again." },
                        new SelectListItem() { Text = "Duplicate Answer", Value = "It appears that you have (or someone else has) submitted another answer that is identical or better addresses the question." },
                        new SelectListItem() { Text = "Off Topic", Value = "Substantial portions of your answer are unrelated to the question - try again." },
                    },
                    ApproveText = this.UserDetails.PublisherId.HasValue ? "Approve" : "Recommend Approval",
                    CanOnlyRecommendApproval = this.UserDetails.PublisherId.HasValue == false,
                }).ToList();
            model.ReviewedAnswerCount = model.Answers.Count(a => a.ReviewedBy != null);

            // Hide previous approved or rejected answers if there is at least one unreviewed answer.
            model.HideReviewedAnswers = model.ReviewedAnswerCount > 0 && model.Answers.Count(a => a.ReviewedBy == null) > 0;

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
            AnswerStatus previousStatus = answer.Status;
            if (answer.Details != answerDetails && answer.OriginalDetails == null)
                answer.OriginalDetails = answer.Details;
            answer.Details = answerDetails;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            answer.RejectionReason = null;
            answer.PublisherId = this.UserDetails.PublisherId;
            if (answer.PublisherId == null)
                answer.Status = AnswerStatus.Recommended;    // No publisher means that the user can only recommend approval.
            else
                answer.Status = AnswerStatus.Approved;
            this.DataContext.SubmitChanges();

            // Log an event.
            LogEvent(answer.PublisherId == null ? EventType.RecommendAnswer : EventType.PublishAnswer, this.User.Id, new { AnswerId = answerId, Details = answerDetails });
            this.DataContext.SubmitChanges();

            // Update the search index.
            SearchIndexer.UpdateQuestion(answer.Question);

            if (answer.Status == AnswerStatus.Approved)
            {

                // Only send emails if the answer's status has changed.
                if (previousStatus != answer.Status)
                {

                    // Send a message to the lawyer that answered the question.
                    var answerPublishedMessage = new Email.AnswerApprovedMessage();
                    answerPublishedMessage.To.Add(answer.CreatedByUser.EmailDisplayName);
                    answerPublishedMessage.ReplyToList.Add(this.User.EmailAddress);
                    answerPublishedMessage.Name = answer.CreatedByUser.EmailGreeting;
                    answerPublishedMessage.Question = answer.Question.Title;
                    answerPublishedMessage.QuestionUri = answer.Question.Uri;
                    answerPublishedMessage.AnswerHtml = StringUtilities.ConvertTextToHtml(answer.Details);
                    answerPublishedMessage.UnansweredQuestionCount = this.DataContext.Questions.
                        Count(q => q.Approved == true && q.Answers.Count(a => a.Status == AnswerStatus.Approved) == 0);
                    answerPublishedMessage.Send();

                    // Send a message to the user who asked the question.
                    var questionAnsweredMessage = new Email.QuestionAnsweredMessage();
                    questionAnsweredMessage.To.Add(answer.Question.CreatedByUser.EmailAddress);
                    questionAnsweredMessage.Question = answer.Question.Title;
                    questionAnsweredMessage.DetailsHtml = StringUtilities.ConvertTextToHtml(answer.Question.Details);
                    questionAnsweredMessage.QuestionUri = answer.Question.Uri;
                    questionAnsweredMessage.AnswerHtml = StringUtilities.ConvertTextToHtml(answer.Details);
                    questionAnsweredMessage.Send();

                }

                // Recalculate the number of answered questions in the category.
                UpdateCategory(answer.Question.CategoryId);

            }

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
            if (reason.Length > 20000)
                return new StatusPlusTextResult(400, "Your rejection reason is too long.");

            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).SingleOrDefault();
            if (answer == null)
                return new StatusPlusTextResult(400, "The answer doesn't exist.");
            AnswerStatus previousStatus = answer.Status;
            answer.Status = AnswerStatus.Rejected;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            answer.RejectionReason = reason;
            this.DataContext.SubmitChanges();

            // Log an event.
            LogEvent(EventType.RejectAnswer, this.User.Id, new { AnswerId = answerId, Reason = reason });
            this.DataContext.SubmitChanges();

            // Only send an email if the answer's status has changed.
            if (previousStatus != answer.Status)
            {
                // Send a message to the lawyer saying their answer has been rejected.
                var rejectionMessage = new Email.AnswerRejectedMessage();
                rejectionMessage.To.Add(answer.CreatedByUser.EmailDisplayName);
                rejectionMessage.ReplyToList.Add(this.User.EmailAddress);
                rejectionMessage.Name = answer.CreatedByUser.EmailGreeting;
                rejectionMessage.Question = answer.Question.Title;
                rejectionMessage.AdminQuestionUri = answer.Question.AdminUri;
                rejectionMessage.AnswerDate = answer.CreatedOn.ToString("d MMM");
                rejectionMessage.ReasonHtml = StringUtilities.ConvertTextToHtml(reason);
                rejectionMessage.Send();
            }

            // Update the search index.
            SearchIndexer.UpdateQuestion(answer.Question);

            // Recalculate the number of answered questions in the category.
            UpdateCategory(answer.Question.CategoryId);

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
        /// Displays the reference materials page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReferenceMaterials()
        {
            // Ensure the user is allowed to vet questions.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");
            return View();
        }

        /// <summary>
        /// Displays the terminology page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Terminology()
        {
            // Ensure the user is allowed to vet questions.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");
            return View();
        }

        private const DayOfWeek WeekBegins = DayOfWeek.Monday;
        private const int DaysInWeek = 7;

        /// <summary>
        /// Displays the admin page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Admin(DateTime? weekBeginning = null)
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");
            return View();
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

        private class AnswerDeserializer
        {
            public int AnswerId { get; set; }
        }

        private class LeaderboardAnswer
        {
            public int UserId { get; set; }
            public EventType EventType { get; set; }
        }

        /// <summary>
        /// Displays the leaderboard page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Leaderboard(DateTime? weekBeginning = null)
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAnswerQuestions == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");

            var viewModel = new LeaderboardViewModel();
            DateTime startDate;
            if (weekBeginning.HasValue == false)
                startDate = DateTime.Now.AddDays(-(((int)DateTime.Now.DayOfWeek - (int)WeekBegins) % DaysInWeek) - DaysInWeek).Date;
            else
                startDate = weekBeginning.Value;
            viewModel.StartDate = startDate.ToLongDateString();
            viewModel.EndDate = startDate.AddDays(DaysInWeek - 1).ToLongDateString();
            viewModel.PreviousLink = string.Format("?weekBeginning={0:yyyy-MM-dd}", startDate.AddDays(-DaysInWeek));
            if (startDate.AddDays(DaysInWeek) < DateTime.Now)
                viewModel.NextLink = string.Format("?weekBeginning={0:yyyy-MM-dd}", startDate.AddDays(DaysInWeek));
            var events = this.DataContext.Events.Where(e => e.EventDate >= new DateTimeOffset(startDate) && e.EventDate < new DateTimeOffset(startDate.AddDays(DaysInWeek))).ToList();
            viewModel.Rows = events.GroupBy(e => e.User).Select(grouping => new LeaderboardRow()
            {
                UserId = grouping.Key.UserId,
                Name = grouping.Key.DisplayName,
                RejectQuestion = grouping.Count(e => e.EventType == EventType.RejectQuestion),
                ApproveQuestion = grouping.Count(e => e.EventType == EventType.ApproveQuestion),
                CreateAnswer = grouping.Count(e => e.EventType == EventType.CreateAnswer),
                RejectAnswer = grouping.Count(e => e.EventType == EventType.RejectAnswer),
                RecommendAnswer = grouping.Count(e => e.EventType == EventType.RecommendAnswer),
                PublishAnswer = grouping.Count(e => e.EventType == EventType.PublishAnswer),
            }).OrderBy(row => row.Name).ToList();

            // Get answer details for event type RejectAnswer, RecommendAnswer and PublishAnswer.
            var answerEvents = events.Where(e => e.EventType == EventType.RejectAnswer || e.EventType == EventType.RecommendAnswer || e.EventType == EventType.PublishAnswer).ToList();
            var answerIds = answerEvents.Select(ae => Newtonsoft.Json.JsonConvert.DeserializeObject<AnswerDeserializer>(ae.Details).AnswerId).Distinct().ToList();
            var answers = this.DataContext.Answers.Where(a => answerIds.Contains(a.AnswerId)).ToList();
            var amalgam = answers.Join(answerEvents,
                a => a.AnswerId,
                ae => Newtonsoft.Json.JsonConvert.DeserializeObject<AnswerDeserializer>(ae.Details).AnswerId,
                (a, ae) => new LeaderboardAnswer { EventType = ae.EventType, UserId = a.CreatedByUserId });
            foreach (var row in viewModel.Rows)
            {
                row.MyAnswersRejected = amalgam.Count(a => a.UserId == row.UserId && a.EventType == EventType.RejectAnswer);
                row.MyAnswersRecommended = amalgam.Count(a => a.UserId == row.UserId && a.EventType == EventType.RecommendAnswer);
                row.MyAnswersPublished = amalgam.Count(a => a.UserId == row.UserId && a.EventType == EventType.PublishAnswer);
            }

            return View(viewModel);
        }

        /// <summary>
        /// Updates the category stats.
        /// </summary>
        /// <param name="categoryId"> The ID of the category that should be updated. </param>
        private void UpdateCategory(int categoryId)
        {
            // Recalculate the number of approved and answered questions in the category.
            this.DataContext.ExecuteCommand(@"
                UPDATE Category
                SET ApprovedQuestionCount = (
                    SELECT COUNT(*)
                    FROM Question
                    WHERE Question.CategoryId = Category.CategoryId
                        AND Approved = 1),
                AnsweredQuestionCount = (
	                SELECT COUNT(*) FROM Question
	                WHERE Question.CategoryId = Category.CategoryId
	                AND Question.Approved = 1
	                AND EXISTS (SELECT * FROM Answer WHERE Question.QuestionId = Answer.QuestionId AND Answer.Status = 1)
                )
                WHERE Category.CategoryId = {0}", categoryId);
        }

        /// <summary>
        /// Displays the view users page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ViewUsers(string searchText)
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");

            var viewModel = new ViewUsersViewModel();
            if (searchText != null)
            {
                viewModel.Rows = this.DataContext.Users
                    .Where(u => u.EmailAddress.Contains(searchText))
                    .ToList()
                    .Select(u => new ViewUsersRowViewModel()
                    {
                        UserId = u.UserId,
                        Email = u.EmailAddress,
                        Region = u.Region.Name,
                        StartDate = u.CreatedOn.ToString("d MMM yyyy"),
                    });
            }
            return View(viewModel);
        }

        /// <summary>
        /// Displays the view user page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ViewUser(int userId)
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");

            var user = this.DataContext.Users.Where(u => u.UserId == userId).Single();

            var viewModel = new ViewUserViewModel();
            viewModel.UserId = user.UserId;
            viewModel.Email = user.EmailAddress;
            viewModel.Region = user.Region.Name;
            viewModel.StartDate = user.CreatedOn.ToString("d MMM yyyy");
            viewModel.CommunityServicesCardNumber = user.CommunityServicesCardNumber.HasValue ? user.CommunityServicesCardNumber.Value.ToString() : "N/A";
            viewModel.CanPublishAnswers = user.Publisher != null;
            viewModel.Publisher = user.Publisher == null ? "" : user.Publisher.Name;
            viewModel.CanAdminister = user.CanAdminister;
            viewModel.CanAnswerQuestions = user.CanAnswerQuestions;
            viewModel.CanVetAnswers = user.CanVetAnswers;
            viewModel.CanVetQuestions = user.CanVetQuestions;
            viewModel.CanVetLawyers = user.CanVetLawyers;
            viewModel.LastLoginDate = user.LastLogInDate.HasValue ? user.LastLogInDate.Value.ToString("d MMM yyyy") : "Never";
            viewModel.LoginCount = user.LogInCount;
            viewModel.LoginIpAddress = user.LogInIpAddress;

            if (user.IsRegisteredLawyer)
            {
                viewModel.IsLawyer = true;
                viewModel.Name = user.Lawyer.FullName;
                viewModel.YearOfAdmission = user.Lawyer.YearOfAdmission;
                viewModel.Specialization = user.Lawyer.Category != null ? user.Lawyer.Category.Name : "None";
                viewModel.Employer = user.Lawyer.EmployerName;
                if (user.Lawyer.Approved)
                    viewModel.ApprovalStatus = "Approved";
                else if (user.Lawyer.RejectionReason != null)
                    viewModel.ApprovalStatus = string.Format("Rejected for reason: {0}", user.Lawyer.RejectionReason);
                else
                    viewModel.ApprovalStatus = "Pending";
            }
            return View(viewModel);
        }

        /// <summary>
        /// Edits a user.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ViewUser(ViewUserViewModel model)
        {
            // Ensure the user is allowed to administer the site.
            if (this.User.CanAdminister == false)
                return new StatusPlusTextResult(403, "Your account is not authorized to view this page.");

            var user = this.DataContext.Users.Where(u => u.UserId == model.UserId).Single();
            user.CanAnswerQuestions = model.CanAnswerQuestions;
            user.CanVetAnswers = model.CanVetAnswers;
            user.CanVetLawyers = model.CanVetLawyers;
            user.CanVetQuestions = model.CanVetQuestions;
            this.DataContext.SubmitChanges();

            return RedirectToAction("ViewUser", new { userId = model.UserId, alert = "updated" });
        }
    }
}
