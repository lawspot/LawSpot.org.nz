﻿using System;
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
    public class RegisterMessage : EmailTemplate
    {
        public RegisterMessage()
        {
            this.TemplateFilePath = "Register.xslt";
            this.Subject = "Thank you for registering with LawSpot";
        }

        /// <summary>
        /// Indicates that a lawyer registered, not a regular user.
        /// </summary>
        public void UseLawyerRegistrationTemplate()
        {
            this.TemplateFilePath = "LawyerRegister.xslt";
            this.Subject = "Please confirm your email address with LawSpot";
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

        /// <summary>
        /// The user registered as part of asking a question.
        /// </summary>
        [ExposeToXslt]
        public bool AskedQuestion { get; set; }
    }

}