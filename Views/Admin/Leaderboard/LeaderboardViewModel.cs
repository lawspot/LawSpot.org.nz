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
        public string Name { get; set; }
        public int Unreviewed { get; set; }
        public int Rejected { get; set; }
        public int RecommendedForApproval { get; set; }
        public int Approved { get; set; }
        public int Total { get; set; }
    }
}