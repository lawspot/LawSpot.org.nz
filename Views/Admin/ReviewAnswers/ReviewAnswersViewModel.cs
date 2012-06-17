using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin
{
    public class ReviewAnswersViewModel
    {
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> CannedRejectionReasons { get; set; }
        public PagedListView<AnswerViewModel> Answers { get; set; }
    }

    public enum ReviewAnswersFilter
    {
        Unreviewed,
        Approved,
        ApprovedByMe,
        Rejected,
        RejectedByMe,
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
        public string AnsweredBy { get; set; }
        public string ReferencesHtml { get; set; }
    }
}
