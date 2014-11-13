using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Lawspot.Shared;

namespace Lawspot.Views.Admin
{
    public class ReferralsViewModel
    {
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> CategoryOptions { get; set; }
        public IEnumerable<SelectListItem> FilterOptions { get; set; }
        public IEnumerable<SelectListItem> SortOptions { get; set; }
        public string Search { get; set; }
        public PagedListView<ReferralViewModel> Questions { get; set; }
    }

    public enum ReferralFilter
    {
        All,
        Accepted,
        AwaitingAcceptance,
    }

    public class ReferralViewModel
    {
        public int QuestionId { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string DateAndTime { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool Accepted { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
        public string OtherPartyName { get; set; }
    }
}
