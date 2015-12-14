using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Ask
{
    public class QuestionViewModel
    {
        public bool AllowQuestions { get; set; }
        public string CannotAskQuestionsMessage { get; set; }

        [Required(ErrorMessage = "Please enter your question.")]
        [StringLength(150, ErrorMessage = "Your question is too long.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please explain your question in detail.")]
        [StringLength(600, ErrorMessage = "Your details are too long.")]
        public string Details { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        [MustBeTrue(ErrorMessage = "You must accept the terms & conditions to proceed.")]
        public bool Agreement { get; set; }

        public IEnumerable<SearchSuggestion> Suggestions { get; set; }

        public bool FocusInTitle { get; set; }
        public bool FocusInDetails { get; set; }

        public bool ShowRegistrationOrLogin
        {
            get { return this.Registration != null; }
        }

        public bool ShowRegistration { get; set; }
        public bool FocusInRegistrationEmailAddress { get; set; }
        public Lawspot.Views.Account.RegisterViewModel Registration { get; set; }

        public bool ShowLogin { get { return !ShowRegistration; } }
        public bool FocusInLoginEmailAddress { get; set; }
        public Lawspot.Views.Account.LoginViewModel Login { get; set; }

        public string Hunee { get; set; }   // Honeypot.
    }

    public class SearchSuggestion
    {
        public string Title { get; set; }
        public string Uri { get; set; }
        public string Details { get; set; }
    }
}