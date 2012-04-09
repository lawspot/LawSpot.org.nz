using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class ApproveLawyersViewModel
    {
        public string FullName { get; set; }
        public IEnumerable<LawyerViewModel> Lawyers { get; set; }
    }

    public enum LawyerFilter
    {
        NotYetApproved,
        Approved,
    }

    public enum LawyerSortOrder
    {
        MostRecent,
        FirstPosted,
    }

    public class LawyerViewModel
    {
        public int LawyerId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string YearAdmitted { get; set; }
        public string DateRegistered { get; set; }
    }
}
