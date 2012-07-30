using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Shared;

namespace Lawspot.Views.Browse
{
    public class CategoryPageViewModel : ITopCategories, IRecentAnswers, IMostViewedQuestions
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }

        public IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
        public PagedListView<AnsweredQuestionViewModel> RecentAnswers { get; set; }
        public IEnumerable<QuestionViewModel> MostViewedQuestions { get; set; }
    }
}