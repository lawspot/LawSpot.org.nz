using System;
using System.Collections.Generic;
using System.Linq;
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
        public ActionResult AnswerQuestions()
        {
            var model = new AnswerQuestionsViewModel();
            model.Questions = this.DataContext.Questions
                .OrderBy(q => q.CreatedOn)
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
