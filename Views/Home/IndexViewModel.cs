using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Views.Home
{
    public class IndexViewModel
    {
        public IEnumerable<QuestionViewModel> RecentQuestions { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
    }

    public class QuestionViewModel
    {
        public string Uri { get; set; }
        public string AvatarUri { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Asker { get; set; }
        public string Ago { get; set; }
    }

    public class CategoryViewModel
    {
        public string Uri { get; set; }
        public string Name { get; set; }
    }
}