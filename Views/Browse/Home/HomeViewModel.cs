using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Backend;

namespace Lawspot.Views.Browse
{
    public class BrowseViewModel
    {
        public IEnumerable<CategoryViewModel> TopCategories1 { get; set; }
        public IEnumerable<CategoryViewModel> TopCategories2 { get; set; }
    }

    public class HomeViewModel : BrowseViewModel
    {
        public IEnumerable<AnsweredQuestionViewModel> RecentAnswers { get; set; }
    }

    public class AnsweredQuestionViewModel
    {
        public string Uri { get; set; }
        public string AvatarUri { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string AnsweredBy { get; set; }
        public string AnsweredHoursAgo { get; set; }
    }
}