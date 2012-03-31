using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Ask
{
    public class QuestionViewModel
    {
        [Required(ErrorMessage = "Please enter your question.")]
        [StringLength(150, ErrorMessage = "Your question is too long.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please explain your question in detail.")]
        [StringLength(600, ErrorMessage = "Your details are too long.")]
        public string Details { get; set; }

        public int CategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        public bool Agreement { get; set; }
    }
}