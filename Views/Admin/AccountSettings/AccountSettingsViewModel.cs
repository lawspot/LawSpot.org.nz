using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class AccountSettingsViewModel
    {
        [Required(ErrorMessage = "Please enter an email address.")]
        [StringLength(200, ErrorMessage = "The email address is too long.")]
        [RegularExpression(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$",
            ErrorMessage = "The email address is not valid.")]
        public string EmailAddress { get; set; }

        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }

        public bool ExpandEmailAddressSection { get; set; }
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
}