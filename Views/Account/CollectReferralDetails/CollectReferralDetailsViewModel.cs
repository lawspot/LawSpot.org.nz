using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{
    public class CollectReferralDetailsViewModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        [StringLength(50, ErrorMessage = "Your first name is too long.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        [StringLength(50, ErrorMessage = "Your last name is too long.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter your phone number.")]
        [StringLength(50, ErrorMessage = "Your phone number is too long.")]
        public string PhoneNumber { get; set; }
    }
}