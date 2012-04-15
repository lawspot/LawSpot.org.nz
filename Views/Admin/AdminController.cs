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
            var filterValue = QuestionFilter.Unanswered;
            if (filter != null)
                filterValue = (QuestionFilter)Enum.Parse(typeof(QuestionFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "All", Value = QuestionFilter.All.ToString(), Selected = filterValue == QuestionFilter.All },
                new SelectListItem() { Text = "Unanswered", Value = QuestionFilter.Unanswered.ToString(), Selected = filterValue == QuestionFilter.Unanswered },
                new SelectListItem() { Text = "Answered", Value = QuestionFilter.Answered.ToString(), Selected = filterValue == QuestionFilter.Answered },
                new SelectListItem() { Text = "Answered by Me", Value = QuestionFilter.AnsweredByMe.ToString(), Selected = filterValue == QuestionFilter.AnsweredByMe },
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
                case QuestionFilter.Unanswered:
                    questions = questions.Where(q => q.Answers.Any() == false);
                    break;
                case QuestionFilter.Answered:
                    questions = questions.Where(q => q.Answers.Any() == true);
                    break;
                case QuestionFilter.AnsweredByMe:
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
        public ActionResult PostAnswer(int questionId, string answerText)
        {
            // Ensure the user is allow to answer questions.
            if (this.User.CanAnswerQuestions == false)
                return new HttpStatusCodeResult(403);

            // Create a new answer for the question.
            var answer = new Answer();
            answer.CreatedOn = DateTimeOffset.Now;
            answer.Details = answerText;
            answer.CreatedByUserId = this.User.Id;
            answer.QuestionId = questionId;
            this.DataContext.Answers.InsertOnSubmit(answer);
            this.DataContext.SubmitChanges();

            return new EmptyResult();
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
                    Name = l.FullName,
                    EmailAddress = l.User.EmailAddress,
                    DateRegistered = l.CreatedOn.ToString("d MMM yyyy"),
                    YearAdmitted = l.YearOfAdmission,
                });

            return View(model);
        }

        /// <summary>
        /// Approves a lawyer.
        /// </summary>
        /// <param name="questionId"> The ID of the lawyer to approve. </param>
        /// <param name="title"> The new title of the question. </param>
        /// <param name="details"> The new question details. </param>
        /// <param name="categoryId"> The new category ID. </param>
        [HttpPost]
        public ActionResult ApproveLawyer(int lawyerId)
        {
            // Ensure the user is allow to vet lawyers.
            if (this.User.CanVetLawyers == false)
                return new HttpStatusCodeResult(403);

            var lawyer = this.DataContext.Lawyers.Where(l => l.LawyerId == lawyerId).Single();
            lawyer.Approved = true;
            lawyer.ReviewDate = DateTimeOffset.Now;
            lawyer.ReviewedByUserId = this.User.Id;
            lawyer.User.CanAnswerQuestions = true;
            this.DataContext.SubmitChanges();

            return new EmptyResult();
        }

        /// <summary>
        /// Rejects a lawyer.
        /// </summary>
        /// <param name="questionId"> The ID of the lawyer to reject. </param>
        [HttpPost]
        public ActionResult RejectLawyer(int lawyerId)
        {
            // Ensure the user is allow to vet lawyers.
            if (this.User.CanVetLawyers == false)
                return new HttpStatusCodeResult(403);

            var lawyer = this.DataContext.Lawyers.Where(l => l.LawyerId == lawyerId).Single();
            lawyer.Approved = false;
            lawyer.ReviewDate = DateTimeOffset.Now;
            lawyer.ReviewedByUserId = this.User.Id;
            lawyer.User.CanAnswerQuestions = false;
            this.DataContext.SubmitChanges();

            return new EmptyResult();
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
            var filterValue = QuestionFilter.Unanswered;
            if (filter != null)
                filterValue = (QuestionFilter)Enum.Parse(typeof(QuestionFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "All", Value = QuestionFilter.All.ToString(), Selected = filterValue == QuestionFilter.All },
                new SelectListItem() { Text = "Unanswered", Value = QuestionFilter.Unanswered.ToString(), Selected = filterValue == QuestionFilter.Unanswered },
                new SelectListItem() { Text = "Answered", Value = QuestionFilter.Answered.ToString(), Selected = filterValue == QuestionFilter.Answered },
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
                case QuestionFilter.Unanswered:
                    questions = questions.Where(q => q.Answers.Any() == false);
                    break;
                case QuestionFilter.Answered:
                    questions = questions.Where(q => q.Answers.Any() == true);
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
        public ActionResult ApproveQuestion(int questionId, string title, string details, int categoryId)
        {
            // Ensure the user is allow to vet questions.
            if (this.User.CanVetQuestions == false)
                return new HttpStatusCodeResult(403);

            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).Single();
            question.Title = title;
            question.Details = details;
            question.CategoryId = categoryId;
            question.Approved = true;
            question.ReviewDate = DateTimeOffset.Now;
            question.ReviewedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();

            return new EmptyResult();
        }

        /// <summary>
        /// Rejects a question.
        /// </summary>
        /// <param name="questionId"> The ID of the question to reject. </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RejectQuestion(int questionId)
        {
            // Ensure the user is allow to vet questions.
            if (this.User.CanVetQuestions == false)
                return new HttpStatusCodeResult(403);

            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).Single();
            question.Approved = false;
            question.ReviewDate = DateTimeOffset.Now;
            question.ReviewedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();

            return new EmptyResult();
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
            var filterValue = AnswerFilter.Unreviewed;
            if (filter != null)
                filterValue = (AnswerFilter)Enum.Parse(typeof(AnswerFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "Unreviewed", Value = AnswerFilter.Unreviewed.ToString(), Selected = filterValue == AnswerFilter.Unreviewed },
                new SelectListItem() { Text = "Approved", Value = AnswerFilter.Approved.ToString(), Selected = filterValue == AnswerFilter.Approved },
                new SelectListItem() { Text = "Rejected", Value = AnswerFilter.Rejected.ToString(), Selected = filterValue == AnswerFilter.Rejected },
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
                case AnswerFilter.Unreviewed:
                    answers = answers.Where(a => a.ReviewDate == null);
                    break;
                case AnswerFilter.Approved:
                    answers = answers.Where(a => a.Approved == true && a.ReviewDate != null);
                    break;
                case AnswerFilter.Rejected:
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
        public ActionResult ApproveAnswer(int answerId, string answerDetails)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                return new HttpStatusCodeResult(403);

            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).Single();
            answer.Details = answerDetails;
            answer.Approved = true;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();

            return new EmptyResult();
        }

        /// <summary>
        /// Rejects an answer.
        /// </summary>
        /// <param name="answerId"> The ID of the answer to reject. </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RejectAnswer(int answerId)
        {
            // Ensure the user is allow to vet answers.
            if (this.User.CanVetAnswers == false)
                return new HttpStatusCodeResult(403);

            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).Single();
            answer.Approved = false;
            answer.ReviewDate = DateTimeOffset.Now;
            answer.ReviewedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();

            return new EmptyResult();
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
