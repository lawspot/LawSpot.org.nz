using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class ReviewQuestionsViewModel
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public IEnumerable<QuestionViewModel> Questions { get; set; }
    }
}
