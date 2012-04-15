using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class ReviewAnswersViewModel
    {
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<AnswerViewModel> Answers { get; set; }
    }

    public enum AnswerFilter
    {
        Unreviewed,
        Approved,
        Rejected,
    }

    public enum AnswerSortOrder
    {
        MostRecent,
        FirstPosted,
    }

    public class AnswerViewModel
    {
        public int AnswerId { get; set; }
        public string DateAndTime { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string CategoryName { get; set; }
        public string Answer { get; set; }
    }
}
