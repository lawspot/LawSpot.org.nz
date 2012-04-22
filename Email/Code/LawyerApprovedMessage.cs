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
    /// The email sent to a lawyer when their account has been successfully verified.
    /// </summary>
    public class LawyerApprovedMessage : EmailTemplate
    {
        public LawyerApprovedMessage()
        {
            this.TemplateFilePath = "LawyerApproved.xslt";
            this.Subject = "Your lawyer account now activated";
        }

        /// <summary>
        /// The name of the lawyer.
        /// </summary>
        [ExposeToXslt]
        public string Name { get; set; }
    }

}