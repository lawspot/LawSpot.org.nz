using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Views.Browse
{
    public class BrowsePageViewModel : IRecentAnswers, IMostViewedQuestions
    {
        public IEnumerable<CategoryViewModel> Categories1 { get; set; }
        public IEnumerable<CategoryViewModel> Categories2 { get; set; }

        public IEnumerable<AnsweredQuestionViewModel> RecentAnswers { get; set; }
        public IEnumerable<QuestionViewModel> MostViewedQuestions { get; set; }
    }
}