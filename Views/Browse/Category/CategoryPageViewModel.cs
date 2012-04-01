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

    public class CategoryViewModel
    {
        public string Uri { get; set; }
        public string Name { get; set; }
    }
}