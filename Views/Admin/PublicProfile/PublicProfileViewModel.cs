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
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string WebsiteUri { get; set; }
        public string PhysicalAddress { get; set; }
        public HttpPostedFileBase Logo { get; set; }
        public string LogoUri { get; set; }
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