using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{
    public class ForgotPasswordViewModel
    {
        public string EmailAddress { get; set; }
    }
}