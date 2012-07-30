using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your email address.")]
        [StringLength(200, ErrorMessage = "Your email address is too long.")]
        [RegularExpression(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$",
            ErrorMessage = "Your email address is not valid.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Your password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Your two passwords do not match.")]
        public string ConfirmPassword { get; set; }

        public int RegionId { get; set; }
        public IEnumerable<SelectListItem> Regions { get; set; }

        [RegularExpression(@"^(?:\s*[0-9]\s*){9}$", ErrorMessage = "The community services card number should be nine digits; you can find it on the lower left corner of your card.")]
        public string CommunityServicesCardNumber { get; set; }

        [MustBeTrue(ErrorMessage = "You must agree to the terms and conditions.")]
        public bool Agreement { get; set; }

        /// <summary>
        /// The URL to redirect to once login is successful.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}