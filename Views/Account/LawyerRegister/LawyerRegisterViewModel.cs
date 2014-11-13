using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Lawspot.Views.Account
{
    public class LawyerRegisterViewModel : RegisterViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select the year you received a license to practice law in NZ.")]
        public int YearAdmitted { get; set; }
        public IEnumerable<SelectListItem> AdmissionYears { get; set; }

        public int SpecialisationCategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        [StringLength(100, ErrorMessage = "Your employer name is too long.")]
        public string EmployerName { get; set; }
    }
}