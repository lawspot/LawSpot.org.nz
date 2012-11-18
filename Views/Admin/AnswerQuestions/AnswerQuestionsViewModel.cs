using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin.AnswerQuestions
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
        public string CategoryName { get; set; }

        // There is:
        // a) A draft by another member that was updated within the last two days.
        // b) A rejected answer by another member that was reviewed within the last two days.
        // c) An answer that is not rejected that was created by another member.
        public bool AnsweredByAnother { get; set; }

        // The current user has posted an answer to the question.
        public bool AnsweredByMe { get; set; }
    }
}
