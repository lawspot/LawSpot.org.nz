using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Views.Browse;

namespace Lawspot.Controllers
{
    public class BrowseController : BaseController
    {
        /// <summary>
        /// Displays the home page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Home()
        {
            var model = new HomeViewModel();
            PopulateHomeViewModel(model, null);
            return View(model);
        }

        /// <summary>
        /// Displays the category page.
        /// </summary>
        /// <param name="slug"> The slug identifying the category. </param>
        /// <returns></returns>
        public ActionResult Category(string slug)
        {
            var category = this.DataContext.Categories.Where(c => c.Slug == slug).SingleOrDefault();
            if (category == null)
                throw new HttpException(404, "Category not found.");
            
            var model = new CategoryPageViewModel();
            model.CategoryId = category.CategoryId;
            model.Name = category.Name;
            PopulateHomeViewModel(model, category.CategoryId);
            return View(model);
        }

        /// <summary>
        /// Displays the question page.
        /// </summary>
        /// <param name="id"> The identifier of the question. </param>
        /// <returns></returns>
        public ActionResult Question(int id)
        {
            var question = this.DataContext.Questions.Where(q => q.QuestionId == id).SingleOrDefault();
            if (question == null)
                throw new HttpException(404, "Question not found.");

            var model = new QuestionPageViewModel();
            PopulateBrowseViewModel(model);
            return View(model);
        }

        private void PopulateHomeViewModel(HomeViewModel model, int? categoryId)
        {
            IEnumerable<Question> filteredQuestions = this.DataContext.Questions;
            if (categoryId != null)
                filteredQuestions = filteredQuestions.Where(q => q.CategoryId == categoryId.Value);
            model.RecentQuestions = filteredQuestions.OrderByDescending(q => q.CreatedOn).Take(5).ToList().Select(q => new QuestionViewModel()
            {
                Uri = string.Format("/questions/{0}", q.QuestionId),
                Title = q.Title,
                Details = q.Details.Length > 100 ? q.Details.Substring(0, 100) : q.Details,
                AvatarUri = "/shared/images/default-avatar.jpg",
                Ago = string.Format("{0} hours", Math.Round(DateTime.Now.Subtract(q.CreatedOn).TotalHours))
            });
            PopulateBrowseViewModel(model);
        }

        private void PopulateBrowseViewModel(BrowseViewModel model)
        {
            var topCategories = this.DataContext.Categories.OrderBy(c => c.Name).Select(c => new CategoryViewModel()
            {
                Uri = string.Format("/categories/{0}", c.Slug),
                Name = c.Name,
            });
            model.TopCategories1 = topCategories.Take(topCategories.Count() / 2);
            model.TopCategories2 = topCategories.Skip(topCategories.Count() / 2);
        }

    }
}
