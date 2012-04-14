using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{
    public class LawyerThankYouViewModel
    {
        public string EmailAddress { get; set; }
        public bool Registered { get; set; }
    }
}