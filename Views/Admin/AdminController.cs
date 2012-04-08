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
        /// All methods on this controller require that the logged in user be an approved lawyer.
        /// </summary>
        /// <param name="filterContext"></param>
        protected override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (this.User == null)
                filterContext.Result = new HttpUnauthorizedResult();
            else if (this.User.IsLawyer == false)
                filterContext.Result = new HttpStatusCodeResult(403);
        }

        [HttpGet]
        public ActionResult AnswerQuestions(string category, string filter, string sort)
        {
            // Get the lawyer details.
            var lawyer = this.DataContext.Users.Single(u => u.EmailAddress == this.User.EmailAddress).Lawyers.Single();

            var model = new AnswerQuestionsViewModel();
            model.FullName = string.Format("{0} {1}", lawyer.FirstName, lawyer.LastName);

            // Categories.
            int categoryId = lawyer.SpecialisationCategoryId ?? 0;
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
            var sortValue = SortOrder.FirstPosted;
            if (sort != null)
                sortValue = (SortOrder)Enum.Parse(typeof(SortOrder), sort, true);
            model.SortOptions = new SelectListItem[]
            {
                new SelectListItem() { Text = "First Posted", Value = SortOrder.FirstPosted.ToString(), Selected = sortValue == SortOrder.FirstPosted },
                new SelectListItem() { Text = "Most Recent", Value = SortOrder.MostRecent.ToString(), Selected = sortValue == SortOrder.MostRecent },
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
                    questions = questions.Where(q => q.Answers.Any(a => a.CreatedByLawyerId == lawyer.LawyerId));
                    break;
            }
            switch (sortValue)
            {
                case SortOrder.FirstPosted:
                    questions = questions.OrderBy(q => q.CreatedOn);
                    break;
                case SortOrder.MostRecent:
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
                    CategoryName = q.Category.Name,
                });
            
            return View(model);
        }

        [HttpPost]
        public ActionResult PostAnswer(int questionId, string answerText)
        {
            // Create a new answer for the question.
            var answer = new Answer();
            answer.CreatedOn = DateTime.Now;
            answer.Details = answerText;
            answer.Lawyer = this.DataContext.Users.Single(u => u.EmailAddress == this.User.EmailAddress).Lawyers.Single();
            answer.QuestionId = questionId;
            this.DataContext.Answers.InsertOnSubmit(answer);
            this.DataContext.SubmitChanges();
            
            return Json("");
        }
    }
}
