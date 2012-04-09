using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.LawyerAdmin
{
    public class AnswerQuestionsViewModel
    {
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public IEnumerable<QuestionViewModel> Questions { get; set; }
    }

    public enum QuestionFilter
    {
        Unanswered,
        All,
        Answered,
        AnsweredByMe,
    }

    public enum QuestionSortOrder
    {
        MostRecent,
        FirstPosted,
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string DateAndTime { get; set; }
        public string CategoryName { get; set; }
    }
}
