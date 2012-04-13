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
        /// All methods on this controller require that the logged in user be a volunteer admin or
        /// a CLC lawyer.
        /// </summary>
        /// <param name="filterContext"> The request context. </param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (this.User == null)
                filterContext.Result = new HttpUnauthorizedResult();
            else if (this.User.IsVolunteerAdmin == false && this.User.IsCLCLawyer == false)
                filterContext.Result = new HttpStatusCodeResult(403);
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
        /// Displays the review lawyers page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewLawyers(string category, string filter, string sort)
        {
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
            var filterValue = LawyerFilter.NotYetApproved;
            if (filter != null)
                filterValue = (LawyerFilter)Enum.Parse(typeof(LawyerFilter), filter, true);
            model.FilterOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "Not Yet Approved", Value = LawyerFilter.NotYetApproved.ToString(), Selected = filterValue == LawyerFilter.NotYetApproved },
                new SelectListItem() { Text = "Approved", Value = LawyerFilter.Approved.ToString(), Selected = filterValue == LawyerFilter.Approved },
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
                case LawyerFilter.Approved:
                    lawyers = lawyers.Where(l => l.Approved == true);
                    break;
                case LawyerFilter.NotYetApproved:
                    lawyers = lawyers.Where(l => l.Approved == false);
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
        public void ApproveLawyer(int lawyerId)
        {
            var lawyer = this.DataContext.Lawyers.Where(l => l.LawyerId == lawyerId).Single();
            lawyer.Approved = true;
            lawyer.ApprovalDate = DateTimeOffset.Now;
            lawyer.ApprovedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();
        }

        /// <summary>
        /// Rejects a lawyer.
        /// </summary>
        /// <param name="questionId"> The ID of the lawyer to reject. </param>
        [HttpPost]
        public void RejectLawyer(int lawyerId)
        {
            var lawyer = this.DataContext.Lawyers.Where(l => l.LawyerId == lawyerId).Single();
            lawyer.Approved = false;
            lawyer.RejectionDate = DateTimeOffset.Now;
            lawyer.RejectedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();
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
        [HttpPost]
        public void ApproveQuestion(int questionId, string title, string details, int categoryId)
        {
            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).Single();
            question.Title = title;
            question.Details = details;
            question.CategoryId = categoryId;
            question.Approved = true;
            question.ApprovalDate = DateTimeOffset.Now;
            question.ApprovedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();
        }

        /// <summary>
        /// Rejects a question.
        /// </summary>
        /// <param name="questionId"> The ID of the question to reject. </param>
        [HttpPost]
        public void RejectQuestion(int questionId)
        {
            var question = this.DataContext.Questions.Where(q => q.QuestionId == questionId).Single();
            question.Approved = false;
            question.RejectionDate = DateTimeOffset.Now;
            question.RejectedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();
        }

        /// <summary>
        /// Displays the review answers page.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReviewAnswers(string category, string sort)
        {
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
        [HttpPost]
        public void ApproveAnswer(int answerId, string answerDetails)
        {
            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).Single();
            answer.Details = answerDetails;
            answer.Approved = true;
            answer.ApprovalDate = DateTimeOffset.Now;
            answer.ApprovedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();
        }

        /// <summary>
        /// Rejects an answer.
        /// </summary>
        /// <param name="answerId"> The ID of the answer to reject. </param>
        [HttpPost]
        public void RejectAnswer(int answerId)
        {
            var answer = this.DataContext.Answers.Where(a => a.AnswerId == answerId).Single();
            answer.Approved = false;
            answer.RejectionDate = DateTimeOffset.Now;
            answer.RejectedByUserId = this.User.Id;
            this.DataContext.SubmitChanges();
        }
    }
}
