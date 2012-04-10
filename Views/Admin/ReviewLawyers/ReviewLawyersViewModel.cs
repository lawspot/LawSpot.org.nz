using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class ReviewLawyersViewModel
    {
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public IEnumerable<LawyerViewModel> Lawyers { get; set; }
    }

    public enum LawyerFilter
    {
        NotYetApproved,
        Approved,
    }

    public enum LawyerSortOrder
    {
        FirstToRegister,
        MostRecent,
    }

    public class LawyerViewModel
    {
        public int LawyerId { get; set; }
        public bool Approved { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public int YearAdmitted { get; set; }
        public string DateRegistered { get; set; }
    }
}
