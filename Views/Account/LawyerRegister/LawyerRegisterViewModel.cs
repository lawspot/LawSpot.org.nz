﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{


    public class LawyerRegisterViewModel : RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        [StringLength(50, ErrorMessage = "Your first name is too long.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        [StringLength(50, ErrorMessage = "Your last name is too long.")]
        public string LastName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select the year you received a license to practice law in NZ.")]
        public int YearAdmitted { get; set; }
        public IEnumerable<SelectListItem> AdmissionYears { get; set; }

        [Range(1, 2, ErrorMessage = "Please indicate whether you are qualified to practise on your own account.")]
        public int CanPublishOption { get; set; }
        public bool CanNotPublish { get; set; }
        public bool CanPublish { get; set; }

        [StringLength(100, ErrorMessage = "Your employer name is too long.")]
        public string EmployerName { get; set; }
    }
}