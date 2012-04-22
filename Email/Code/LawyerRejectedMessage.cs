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
    /// The email sent to a lawyer when their account has been rejected.
    /// </summary>
    public class LawyerRejectedMessage : EmailTemplate
    {
        public LawyerRejectedMessage()
        {
            this.TemplateFilePath = "LawyerRejected.xslt";
            this.Subject = "Sorry, your LawSpot Lawyer account is on hold";
        }

        /// <summary>
        /// The name of the lawyer.
        /// </summary>
        [ExposeToXslt]
        public string Name { get; set; }

        /// <summary>
        /// The reason the question was rejected.
        /// </summary>
        [ExposeToXslt]
        public string Reason { get; set; }
    }

}