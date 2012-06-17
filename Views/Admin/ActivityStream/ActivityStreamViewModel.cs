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
        public int QuestionsAnswered { get; set; }
        public int QuestionsApproved { get; set; }
        public string LastAnswerSubmitted { get; set; }
        public string MayorStatus { get; set; }
    }

}
