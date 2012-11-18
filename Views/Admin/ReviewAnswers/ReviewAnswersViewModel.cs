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
        public PagedListView<QuestionAndAnswerViewModel> Answers { get; set; }
    }

    public enum ReviewAnswersFilter
    {
        Unreviewed,
        Approved,
        ApprovedByMe,
        Rejected,
        RejectedByMe,
        RecommendedForApproval,
    }

    public enum AnswerSortOrder
    {
        MostRecent,
        FirstPosted,
    }

    public class QuestionAndAnswerViewModel
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
        public string DateAndTime { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string IconFileName { get; set; }
    }
}
