using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class AdministerViewModel
    {
        public string LeaderboardStartDate { get; set; }
        public string LeaderboardEndDate { get; set; }
        public IEnumerable<LeaderboardRow> LeaderboardRows { get; set; }
    }

    public class LeaderboardRow
    {
        public string Name { get; set; }
        public int Unreviewed { get; set; }
        public int Rejected { get; set; }
        public int RecommendedForApproval { get; set; }
        public int Approved { get; set; }
        public int Total { get; set; }
    }
}