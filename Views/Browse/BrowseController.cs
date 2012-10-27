using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lawspot.Backend;
using Lawspot.Shared;
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
            PopulateModel(model, answersPageSize: 4);
            return View(model);
        }

        /// <summary>
        /// Displays the browse page.
        /// </summary>
        /// <param name="page"> The page number. </param>
        /// <returns></returns>
        public ActionResult Browse(int page = 1)
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
                    QuestionCount = c.AnsweredQuestionCount,
                });
            model.Categories1 = categories.Take((categories.Count() + 1) / 2);
            model.Categories2 = categories.Skip((categories.Count() + 1) / 2);
            PopulateModel(model, null, page);
            return View(model);
        }

        /// <summary>
        /// Displays the category page.
        /// </summary>
        /// <param name="slug"> The slug identifying the category. </param>
        /// <param name="page"> The page number. </param>
        /// <returns></returns>
        public ActionResult Category(string slug, int page = 1)
        {
            var category = this.DataContext.Categories.Where(c => c.Slug == slug).SingleOrDefault();
            if (category == null)
                throw new HttpException(404, "Category not found");
            
            var model = new CategoryPageViewModel();
            model.CategoryId = category.CategoryId;
            model.Name = category.Name;
            PopulateModel(model, category.CategoryId, page);
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
                throw new HttpException(404, "Question not found");
            if (string.Equals(question.Category.Slug, category, StringComparison.OrdinalIgnoreCase) == false)
                throw new HttpException(404, "Category not found");
            if (question.Approved == false)
                throw new HttpException(404, "Question not approved");

            // Increment the number of views.
            this.DataContext.ExecuteCommand("UPDATE [Question] SET ViewCount = ViewCount + 1 WHERE QuestionId = {0}", question.QuestionId);

            var model = new QuestionPageViewModel();
            model.QuestionId = question.QuestionId;
            model.Title = question.Title;
            model.DetailsHtml = StringUtilities.ConvertTextToHtml(question.Details);
            model.CategoryId = question.CategoryId;
            model.CategoryName = question.Category.Name;
            model.CategoryUrl = question.Category.AbsolutePath;
            model.CreationDate = question.CreatedOn.ToString("d MMM yyyy");
            model.Views = question.ViewCount;
            model.Answers = question.Answers
                .Where(a => a.Status == AnswerStatus.Approved)
                .Select(a => new AnswerViewModel()
            {
                DetailsHtml = StringUtilities.ConvertTextToHtml(a.Details),
            });
            PopulateModel(model, question.CategoryId);
            return View(model);
        }

        /// <summary>
        /// Populates the model object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="categoryId"></param>
        /// <param name="page"></param>
        private void PopulateModel(object model, int? categoryId = null, int answersPage = 1, int answersPageSize = 5)
        {
            if (model is IRecentAnswers)
            {
                IEnumerable<Answer> filteredAnswers = this.DataContext.Answers;
                if (categoryId != null)
                    filteredAnswers = filteredAnswers.Where(a => a.Question.CategoryId == categoryId.Value);
                ((IRecentAnswers)model).RecentAnswers = new PagedListView<AnsweredQuestionViewModel>(filteredAnswers
                    .Where(a => a.Status == AnswerStatus.Approved && a.Question.Approved)
                    .OrderByDescending(a => a.CreatedOn)
                    .Select(a => new AnsweredQuestionViewModel()
                    {
                        Uri = a.Question.AbsolutePath,
                        Title = a.Question.Title,
                        Details = StringUtilities.SummarizeText(a.Details, 150),
                        AnsweredBy = a.Publisher.Name,
                        AnsweredTime = DateTimeOffset.Now.Subtract(a.CreatedOn).TotalHours > 24 ?
                            string.Format("{0:d MMMM yyyy}", a.CreatedOn) :
                            string.Format("{0} hours ago", Math.Round(DateTimeOffset.Now.Subtract(a.CreatedOn).TotalHours)),
                    }), answersPage, answersPageSize, this.Request.Url);
                var lastAnswer = ((IRecentAnswers)model).RecentAnswers.Items.LastOrDefault();
                if (lastAnswer != null)
                    lastAnswer.Last = true;
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
                        QuestionCount = c.AnsweredQuestionCount,
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
                    .Where(q => q.Approved && q.Answers.Any(a => a.Status == AnswerStatus.Approved))
                    .OrderByDescending(q => q.ViewCount)
                    .Take(5)
                    .Select(q => new QuestionViewModel()
                    {
                        Url = q.AbsolutePath,
                        Title = q.Title,
                        AnswerCount = string.Format("{0} answers", q.Answers.Count(a => a.Status == AnswerStatus.Approved)),
                        ViewCount = string.Format("{0} views", q.ViewCount),
                    });
            }
        }

        /// <summary>
        /// Displays the search page.
        /// </summary>
        /// <param name="query"> The text to search for. </param>
        /// <param name="page"> The page number. </param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Search(string query, int page = 1)
        {
            var model = new SearchPageViewModel();
            model.Query = query;
            var hits = SearchIndexer.Search(query ?? string.Empty);
            model.Hits = new PagedListView<SearchResultViewModel>(hits.Join(DataContext.Questions, h => h.ID, q => q.QuestionId, (h, q) =>
                new SearchResultViewModel() {
                    Uri = q.AbsolutePath,
                    Title = q.Title,
                    HighlightsHtml = h.SnippetsHtml,
                    CreatedOn = q.CreatedOn.ToString("d MMMM"),
                    AnswerCount = string.Format("{0} answer(s)", q.Answers.Count()),
                }), page, 10, this.Request.Url);
            return View(model);
        }
    }
}
