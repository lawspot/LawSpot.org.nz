using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Lawspot.Backend;
using Lawspot.Views.LawyerAdmin;

namespace Lawspot.Controllers
{
    public class LawyerAdminController : BaseController
    {
        /// <summary>
        /// All methods on this controller require that the logged in user be an approved lawyer.
        /// </summary>
        /// <param name="filterContext"> The request context. </param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (this.User == null)
                filterContext.Result = new HttpUnauthorizedResult();
            else if (this.User.IsLawyer == false)
                filterContext.Result = new HttpStatusCodeResult(403);
        }

        /// <summary>
        /// Gets the current user's lawyer details.
        /// </summary>
        public Lawyer Lawyer
        {
            get
            {
                var lawyer = (Lawyer)this.HttpContext.Items["Lawyer"];
                if (lawyer == null)
                {
                    lawyer = this.DataContext.Users
                        .Single(u => u.EmailAddress == this.User.EmailAddress)
                        .Lawyers
                        .Single();
                    this.HttpContext.Items["Lawyer"] = lawyer;
                }
                return lawyer;
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
                return new LayoutViewModel()
                {
                    UserFullName = this.Lawyer.FullName,
                    InLawyerAdmin = true,
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
            var model = new AnswerQuestionsViewModel();

            // Categories.
            int categoryId = this.Lawyer.SpecialisationCategoryId ?? 0;
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
                case QuestionFilter.AnsweredByMe:
                    questions = questions.Where(q => q.Answers.Any(a => a.CreatedByLawyerId == this.Lawyer.LawyerId));
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
                .Where(q => q.Approved == true)
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

        [HttpPost]
        public void PostAnswer(int questionId, string answerText)
        {
            // Create a new answer for the question.
            var answer = new Answer();
            answer.CreatedOn = DateTimeOffset.Now;
            answer.Details = answerText;
            answer.Lawyer = this.DataContext.Users.Single(u => u.EmailAddress == this.User.EmailAddress).Lawyers.Single();
            answer.QuestionId = questionId;
            this.DataContext.Answers.InsertOnSubmit(answer);
            this.DataContext.SubmitChanges();
        }
    }
}
