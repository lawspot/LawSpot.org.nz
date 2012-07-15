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
    /// The email sent to lawyers to remind them to answer questions.
    /// </summary>
    public class LawyerReminderMessage : EmailTemplate
    {
        public LawyerReminderMessage()
        {
            this.TemplateFilePath = "LawyerReminder.xslt";
            this.Subject = "Unanswered questions posted on LawSpot";
        }

        /// <summary>
        /// The name of the lawyer.
        /// </summary>
        [ExposeToXslt]
        public string Name { get; set; }


    }

}