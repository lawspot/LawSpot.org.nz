using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Ask
{
    public class QuestionThankModel
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string EmailAddress { get; set; }
    }
}