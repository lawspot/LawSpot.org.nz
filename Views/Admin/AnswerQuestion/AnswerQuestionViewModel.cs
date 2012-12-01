using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin.AnswerQuestion
{
    public class AnswerQuestionViewModel
    {
        public int QuestionId { get; set; }

        public string DateAndTime { get; set; }
        public string Title { get; set; }
        public string DetailsHtml { get; set; }
        public string CategoryName { get; set; }

        public string OriginalTitle { get; set; }
        public string OriginalDetailsHtml { get; set; }

        public string ReviewedBy { get; set; }
        public string ReviewDate { get; set; }

        public bool HideReviewedAnswers { get; set; }
        public int ReviewedAnswerCount { get; set; }

        public IEnumerable<AnswerViewModel> Answers { get; set; }
        
        public string Answer { get; set; }
        public string References { get; set; }
    }

    public class AnswerViewModel
    {
        public string DateAndTime { get; set; }
        public string AnswerHtml { get; set; }
        public string AnsweredBy { get; set; }
        public string ReferencesHtml { get; set; }
        
        public string ReviewedBy { get; set; }
        public bool Draft { get; set; }
        public bool Pending { get; set; }
        public bool Approved { get; set; }
        public bool Rejected { get; set; }
        public bool RecommendedForApproval { get; set; }
        public string RejectionReasonHtml { get; set; }
        public string ReviewDate { get; set; }

        public DateTimeOffset SortKey { get; set; }
    }
}
