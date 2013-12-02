using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class AccountSettingsViewModel
    {
        public string EmailAddress { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }

        public bool ExpandEmailAddressSection { get; set; }
        public bool ExpandPasswordSection { get; set; }
        public bool ExpandNameSection { get; set; }
        public bool ExpandRegionSection { get; set; }
    }

    public class ChangeEmailAddressViewModel
    {
        [Required(ErrorMessage = "Please enter an email address.")]
        [StringLength(200, ErrorMessage = "The email address is too long.")]
        [RegularExpression(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$",
            ErrorMessage = "The email address is not valid.")]
        public string EmailAddress { get; set; }
    }

    public class ChangeNameViewModel
    {
        [StringLength(50, ErrorMessage = "Your first name is too long.")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Your last name is too long.")]
        public string LastName { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Your password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Your two passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}