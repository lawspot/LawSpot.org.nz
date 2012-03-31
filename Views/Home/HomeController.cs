using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Views.Home;

namespace Lawspot.Controllers
{
    public class HomeController : BaseController
    {
        /// <summary>
        /// Displays the home page.
        /// </summary>
        /// <param name="alert"> Indicates that the user should be shown an alert. </param>
        /// <returns></returns>
        public ActionResult Index()
        {
            var model = new IndexViewModel();

            model.RecentQuestions = this.DataContext.Questions.OrderByDescending(q => q.CreatedOn).Take(5).ToList().Select(q => new QuestionViewModel()
            {
                Uri = string.Format("/questions/{0}", q.QuestionId),
                Title = q.Title,
                Details = q.Details.Length > 100 ? q.Details.Substring(0, 100) : q.Details,
                AvatarUri = "/shared/images/default-avatar.jpg",
                Ago = string.Format("{0} hours", Math.Round(DateTime.Now.Subtract(q.CreatedOn).TotalHours))
            });

            var topCategories = this.DataContext.Categories.OrderBy(c => c.Name).Select(c => new CategoryViewModel()
            {
                Uri = string.Format("/categories/{0}", c.CategoryId),
                Name = c.Name,
            });
            model.TopCategories1 = topCategories.Take(topCategories.Count() / 2);
            model.TopCategories2 = topCategories.Skip(topCategories.Count() / 2);

            return View(model);
        }
    }
}
