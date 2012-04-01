using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Views.Browse
{
    public class CategoryPageViewModel : HomeViewModel
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public IEnumerable<QuestionViewModel> MostViewedQuestions { get; set; }
    }

    public class QuestionViewModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string AnswerCount { get; set; }
        public string ViewCount { get; set; }
    }

    public class CategoryViewModel
    {
        public string Uri { get; set; }
        public string Name { get; set; }
    }
}