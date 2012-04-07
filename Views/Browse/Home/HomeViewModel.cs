using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Backend;

namespace Lawspot.Views.Browse
{
    public class HomePageViewModel : ITopCategories, IRecentAnswers
    {
        public IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
        public IEnumerable<AnsweredQuestionViewModel> RecentAnswers { get; set; }
    }
}