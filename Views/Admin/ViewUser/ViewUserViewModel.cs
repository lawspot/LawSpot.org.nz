using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Admin
{
    public class ViewUserViewModel
    {
        // Basic info.
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Region { get; set; }
        public string CommunityServicesCardNumber { get; set; }
        public string StartDate { get; set; }

        // Lawyer info.
        public bool IsLawyer { get; set; }
        public string Name { get; set; }
        public int YearOfAdmission { get; set; }
        public string Specialisation { get; set; }
        public string Employer { get; set; }
        public string ApprovalStatus { get; set; }

        // Login info.
        public string LastLoginDate { get; set; }
        public int LoginCount { get; set; }
        public string LoginIpAddress { get; set; }

        // Permissions.
        public bool CanAnswerQuestions { get; set; }
        public bool CanVetQuestions { get; set; }
        public bool CanVetAnswers { get; set; }
        public bool CanVetLawyers { get; set; }
        public bool CanAdminister { get; set; }
        public bool CanPublishAnswers { get; set; }
        public string Publisher { get; set; }
    }
}