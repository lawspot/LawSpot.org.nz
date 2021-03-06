﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin
{
    public class ReviewQuestionsViewModel
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public string Search { get; set; }
        public IEnumerable<SelectListItem> CannedRejectionReasons { get; set; }
        public PagedListView<ReviewQuestionViewModel> Questions { get; set; }
    }

    public enum ReviewQuestionsFilter
    {
        All,
        Unreviewed,
        Approved,
        ApprovedByMe,
        Rejected,
        RejectedByMe,
        Referred,
    }

    public class ReviewQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string DateAndTime { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ApprovedOrRejected { get; set; }
        public string ReviewedBy { get; set; }
        public string RejectionReasonHtml { get; set; }
    }
}
