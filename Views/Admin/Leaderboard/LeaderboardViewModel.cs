using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class LeaderboardViewModel
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string PreviousLink { get; set; }
        public string NextLink { get; set; }
        public IEnumerable<LeaderboardRow> Rows { get; set; }
    }

    public class LeaderboardRow
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public int RejectQuestion { get; set; }
        public int ApproveQuestion { get; set; }
        public int CreateAnswer { get; set; }
        public int RejectAnswer { get; set; }
        public int MyAnswersRejected { get; set; }
        public int RecommendAnswer { get; set; }
        public int MyAnswersRecommended { get; set; }
        public int PublishAnswer { get; set; }
        public int MyAnswersPublished { get; set; }
    }
}