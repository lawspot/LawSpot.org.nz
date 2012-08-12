using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Your password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Your two passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}