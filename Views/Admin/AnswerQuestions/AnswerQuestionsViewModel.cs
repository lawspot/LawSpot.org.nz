using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin
{
    public class AnswerQuestionsViewModel
    {
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public PagedListView<AnswerQuestionViewModel> Questions { get; set; }
        public IEnumerable<AnswerQuestionViewModel> DraftAnswers { get; set; }
    }

    public enum AnswerQuestionsFilter
    {
        Unanswered,
        All,
        Pending,
        Answered,
        AnsweredByMe,
        SingleQuestion,
    }

    public enum QuestionSortOrder
    {
        MostRecent,
        FirstPosted,
    }

    public class AnswerQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Title { get; set; }
        public string DetailsHtml { get; set; }
        public string ReviewedBy { get; set; }
        public string DateAndTime { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Answer { get; set; }
        public string References { get; set; }
        public string Notification { get; set; }
    }
}
