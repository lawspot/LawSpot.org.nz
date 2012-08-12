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
    /// The email template for when a user indicates they have forgotten their password.
    /// </summary>
    public class ForgotPasswordMessage : EmailTemplate
    {
        public ForgotPasswordMessage()
        {
            this.TemplateFilePath = "ForgotPassword.xslt";
            this.Subject = "Reset your LawSpot password";
        }

        /// <summary>
        /// The URI to the page that will reset the password.
        /// </summary>
        [ExposeToXslt]
        public string ResetPasswordUri { get; set; }

    }

}