using System;
using System.Collections.Generic;
using System.Linq;

namespace Lawspot.Views.Admin
{
    public class AnswerQuestionsViewModel
    {
        public IEnumerable<QuestionViewModel> Questions { get; set; }
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
