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
            var model = new HomePageViewModel();
            PopulateModel(model);
            return View(model);
        }

        /// <summary>
        /// Displays the browse page.
        /// </summary>
        /// <returns></returns>
        public ActionResult Browse()
        {
            // Activate header tab.
            this.BrowseAnswersTabActive = true;

            var model = new BrowsePageViewModel();
            var categories = this.DataContext.Categories
                .OrderBy(c => c.Name)
                .ToList()
                .Select(c => new CategoryViewModel()
                {
                    Uri = c.AbsolutePath,
                    Name = c.Name,
                    AnswerCount = c.Questions.Sum(q => q.Answers.Count(a => a.Approved))
                });
            model.Categories1 = categories.Take((categories.Count() + 1) / 2);
            model.Categories2 = categories.Skip((categories.Count() + 1) / 2);
            PopulateModel(model);
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
                return new HttpStatusCodeResult(404);
            
            var model = new CategoryPageViewModel();
            model.CategoryId = category.CategoryId;
            model.Name = category.Name;
            PopulateModel(model, category.CategoryId);
            return View(model);
        }

        /// <summary>
        /// Displays the question page.
        /// </summary>
        /// <param name="category"> The slug identifying the category. </param>
        /// <param name="slug"> The slug identifying the question. </param>
        /// <returns></returns>
        public ActionResult Question(string category, string slug)
        {
            var question = this.DataContext.Questions.Where(q => q.Slug == slug).SingleOrDefault();
            if (question == null)
                return new HttpStatusCodeResult(404);
            if (string.Equals(question.Category.Slug, category, StringComparison.OrdinalIgnoreCase) == false)
                return new HttpStatusCodeResult(404);
            if (question.Approved == false)
                return new HttpStatusCodeResult(404);

            // Increment the number of views.
            this.DataContext.ExecuteCommand("UPDATE [Question] SET ViewCount = ViewCount + 1 WHERE QuestionId = {0}", question.QuestionId);

            var model = new QuestionPageViewModel();
            model.Title = question.Title;
            model.DetailsHtml = StringUtilities.ConvertTextToHtml(question.Details);
            model.CategoryId = question.CategoryId;
            model.CategoryName = question.Category.Name;
            model.CategoryUrl = question.Category.AbsolutePath;
            model.CreationDate = question.CreatedOn.ToString("d MMM yyyy");
            model.Views = question.ViewCount;
            model.Answers = question.Answers
                .Where(a => a.Approved)
                .Select(a => new AnswerViewModel()
            {
                DetailsHtml = StringUtilities.ConvertTextToHtml(a.Details),
            });
            PopulateModel(model);
            return View(model);
        }

        /// <summary>
        /// Populates the model object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="categoryId"></param>
        private void PopulateModel(object model, int? categoryId = null)
        {
            if (model is IRecentAnswers)
            {
                IEnumerable<Answer> filteredAnswers = this.DataContext.Answers;
                if (categoryId != null)
                    filteredAnswers = filteredAnswers.Where(a => a.Question.CategoryId == categoryId.Value);
                ((IRecentAnswers)model).RecentAnswers = filteredAnswers
                    .Where(a => a.Approved && a.Question.Approved)
                    .OrderByDescending(a => a.CreatedOn)
                    .Take(5)
                    .Select(a => new AnsweredQuestionViewModel()
                    {
                        Uri = a.Question.AbsolutePath,
                        Title = a.Question.Title,
                        Details = a.Details.Length > 100 ? a.Details.Substring(0, 100) : a.Details,
                        AnsweredBy = "Wellington Community Law Centre",
                        AnsweredTime = DateTimeOffset.Now.Subtract(a.CreatedOn).TotalHours > 24 ?
                            string.Format("{0:d MMMM yyyy}", a.CreatedOn) :
                            string.Format("{0} hours ago", Math.Round(DateTimeOffset.Now.Subtract(a.CreatedOn).TotalHours)),
                    });
            }

            if (model is ITopCategories)
            {
                var topCategories = this.DataContext.Categories
                    .OrderByDescending(c => c.Questions.Sum(q => q.Answers.Count()))
                    .Take(10)
                    .OrderBy(c => c.Name)
                    .ToList()
                    .Select(c => new CategoryViewModel()
                    {
                        Uri = c.AbsolutePath,
                        Name = c.Name,
                        AnswerCount = c.Questions.Sum(q => q.Answers.Count(a => a.Approved))
                    });
                ((ITopCategories)model).TopCategories1 = topCategories.Take(5);
                ((ITopCategories)model).TopCategories2 = topCategories.Skip(5);
            }

            if (model is IMostViewedQuestions)
            {
                IEnumerable<Question> filteredQuestions = this.DataContext.Questions;
                if (categoryId.HasValue)
                    filteredQuestions = filteredQuestions.Where(q => q.CategoryId == categoryId);
                ((IMostViewedQuestions)model).MostViewedQuestions = filteredQuestions
                    .Where(q => q.Approved)
                    .OrderByDescending(q => q.ViewCount)
                    .Take(5)
                    .Select(q => new QuestionViewModel()
                    {
                        Url = q.AbsolutePath,
                        Title = q.Title,
                        AnswerCount = string.Format("{0} answers", q.Answers.Count(a => a.Approved)),
                        ViewCount = string.Format("{0} views", q.ViewCount),
                    });
            }
        }
    }
}
