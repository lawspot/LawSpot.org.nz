using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin
{
    public class ActivityStreamViewModel
    {
        public int QuestionsSubmitted { get; set; }
        public int QuestionsPublished { get; set; }
        public string LastQuestionSubmitted { get; set; }
        public int AnswersSubmitted { get; set; }
        public int AnswersPublished { get; set; }
        public string LastAnswerSubmitted { get; set; }
        public int QuestionsReviewed { get; set; }
        public string LastQuestionReviewed { get; set; }
        public int AnswersReviewed { get; set; }
        public string LastAnswerReviewed { get; set; }
        public string MayorStatus { get; set; }
    }

}
