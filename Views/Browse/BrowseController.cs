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
            model.MostViewedQuestions = this.DataContext.Questions
                .Where(q => q.CategoryId == category.CategoryId)
                .OrderByDescending(q => q.ViewCount)
                .Take(5)
                .Select(q => new QuestionViewModel()
            {
                Url = string.Format("/questions/{0}", q.QuestionId),
                Title = q.Title,
                AnswerCount = string.Format("{0} answers", q.Answers.Count()),
                ViewCount = string.Format("{0} views", q.ViewCount),
            });
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

            // Increment the number of views.
            this.DataContext.ExecuteCommand("UPDATE [Question] SET ViewCount = ViewCount + 1 WHERE QuestionId = {0}", id);

            var model = new QuestionPageViewModel();
            model.Title = question.Title;
            model.Details = question.Details;
            model.CategoryId = question.CategoryId;
            model.CategoryName = question.Category.Name;
            model.CategoryUrl = string.Format("/categories/{0}", question.Category.Slug);
            model.CreationDate = question.CreatedOn.ToString("d MMM yyyy");
            model.Views = question.ViewCount;
            model.Answers = question.Answers.Select(a => new AnswerViewModel()
            {
                Details = a.Details,
                AvatarUrl = "http://dummyimage.com/60",
                ProfileUrl = string.Format("/lawyers/{0}", a.Lawyer.LawyerId),
            });
            PopulateBrowseViewModel(model);
            return View(model);
        }

        /// <summary>
        /// Populates the HomeViewModel object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="categoryId"></param>
        private void PopulateHomeViewModel(HomeViewModel model, int? categoryId)
        {
            IEnumerable<Answer> filteredAnswers = this.DataContext.Answers;
            if (categoryId != null)
                filteredAnswers = filteredAnswers.Where(a => a.Question.CategoryId == categoryId.Value);
            model.RecentAnswers = filteredAnswers
                .OrderByDescending(a => a.CreatedOn)
                .Take(5)
                .Select(a => new AnsweredQuestionViewModel()
            {
                Uri = string.Format("/questions/{0}", a.QuestionId),
                Title = a.Question.Title,
                Details = a.Details.Length > 100 ? a.Details.Substring(0, 100) : a.Details,
                AvatarUri = "/shared/images/default-avatar.jpg",
                AnsweredBy = string.Format("{0} {1}", a.Lawyer.FirstName, a.Lawyer.LastName),
                AnsweredHoursAgo = string.Format("{0} hours", Math.Round(DateTime.Now.Subtract(a.CreatedOn).TotalHours))
            });
            PopulateBrowseViewModel(model);
        }

        /// <summary>
        /// Populates the BrowseViewModel object.
        /// </summary>
        /// <param name="model"></param>
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
