using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class ViewUsersViewModel
    {
        public IEnumerable<ViewUsersRowViewModel> Rows { get; set; }
    }

    public class ViewUsersRowViewModel
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Region { get; set; }
        public string StartDate { get; set; }
    }
}