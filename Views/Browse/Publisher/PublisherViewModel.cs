using System;
using System.Collections.Generic;
using System.Linq;
using Lawspot.Backend;
using Lawspot.Shared;

namespace Lawspot.Views.Browse
{
    public class PublisherViewModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string WebsiteUri { get; set; }
        public string PhysicalAddressHtml { get; set; }
        public string PhysicalAddressQuery { get; set; }
        public string DescriptionHtml { get; set; }
        public string LogoUri { get; set; }
        public IEnumerable<QuestionViewModel> RecentlyAnsweredQuestions { get; set; }
    }
}