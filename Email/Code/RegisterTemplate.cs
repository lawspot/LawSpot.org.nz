using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Lawspot.Email
{

    /// <summary>
    /// The email template for when a user registers.
    /// </summary>
    public class RegisterTemplate : EmailTemplate
    {
        public RegisterTemplate()
        {
            this.TemplateFilePath = "Register.xslt";
            this.Subject = "Thanks for registering with LawSpot";
        }

        /// <summary>
        /// The email address of the user that registered.
        /// </summary>
        [ExposeToXslt]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The password that was used to register.
        /// </summary>
        [ExposeToXslt]
        public string Password { get; set; }

        /// <summary>
        /// The URL of the validation link.
        /// </summary>
        [ExposeToXslt]
        public string ValidateEmailUri { get; set; }
    }

}