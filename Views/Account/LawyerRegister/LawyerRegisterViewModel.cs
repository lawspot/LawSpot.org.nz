using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Prolawyers.Views.Account
{
    public class LawyerRegisterViewModel : RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        [StringLength(50, ErrorMessage = "Your first name is too long.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter your last name.")]
        [StringLength(50, ErrorMessage = "Your last name is too long.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter the number of your practising certificate.")]
        [RegularExpression("^[0-9]{4,15}$", ErrorMessage = "Please enter a valid certificate number.")]
        public string PractisingCertNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select the year you received a license to practice law in NZ.")]
        public int YearAdmitted { get; set; }
        public IEnumerable<SelectListItem> AdmissionYears { get; set; }

        public int SpecialisationCategoryId { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }

        [StringLength(100, ErrorMessage = "Your firm name is too long.")]
        public string FirmName { get; set; }
    }
}