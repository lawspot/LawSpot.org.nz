using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class PublicProfileViewModel
    {
        // Can be zero.
        public int PublisherId { get; set; }

        [StringLength(50, ErrorMessage = "The name is too long.")]
        [Required(ErrorMessage = "The name is required.")]
        public string Name { get; set; }

        [StringLength(256, ErrorMessage = "The email address is too long.")]
        [RegularExpression(@"^.+@.+$", ErrorMessage = "The email address is invalid.")]
        public string EmailAddress { get; set; }

        [StringLength(50, ErrorMessage = "The phone number is too long.")]
        [RegularExpression(@"^[^a-zA-Z]+$", ErrorMessage = "Phone numbers cannot contain letters.")]
        public string PhoneNumber { get; set; }

        [StringLength(256, ErrorMessage = "The name is too long.")]
        [RegularExpression(@"^https?://.*$", ErrorMessage = "The website URL must start with http:// or https://.")]
        public string WebsiteUri { get; set; }

        [StringLength(256, ErrorMessage = "The physical address is too long.")]
        public string PhysicalAddress { get; set; }

        [JsonIgnore]
        public HttpPostedFileBase Logo { get; set; }
        public string LogoUri { get; set; }

        [StringLength(250, ErrorMessage = "The description is too long.")]
        public string ShortDescription { get; set; }

        public string LongDescription { get; set; }

        public IEnumerable<PublicProfileCategoryViewModel> Categories { get; set; }
    }

    public class PublicProfileCategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool Selected { get; set; }
    }
}