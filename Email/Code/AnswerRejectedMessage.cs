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
    /// The email sent to a lawyer when their answer has been rejected.
    /// </summary>
    public class AnswerRejectedMessage : EmailTemplate
    {
        public AnswerRejectedMessage()
        {
            this.TemplateFilePath = "AnswerRejected.xslt";
            this.Subject = "Your answer has been rejected";
        }

        /// <summary>
        /// The name of the lawyer.
        /// </summary>
        [ExposeToXslt]
        public string Name { get; set; }

        /// <summary>
        /// The title of the question the user asked.
        /// </summary>
        [ExposeToXslt]
        public string Question { get; set; }

        /// <summary>
        /// The URL of the question in admin.
        /// </summary>
        [ExposeToXslt]
        public string AdminQuestionUri { get; set; }

        /// <summary>
        /// The date the answer was submitted e.g. "4 Aug".
        /// </summary>
        [ExposeToXslt]
        public string AnswerDate { get; set; }

        /// <summary>
        /// The reason the answer was rejected.
        /// </summary>
        [ExposeToXslt]
        public string ReasonHtml { get; set; }
    }

}