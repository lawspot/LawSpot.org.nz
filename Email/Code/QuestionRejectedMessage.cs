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
    /// The email sent to a user when a question of theirs is rejected.
    /// </summary>
    public class QuestionRejectedMessage : EmailTemplate
    {
        public QuestionRejectedMessage()
        {
            this.TemplateFilePath = "QuestionRejected.xslt";
            this.Subject = "Sorry, your question has been rejected on LawSpot";
        }

        /// <summary>
        /// The title of the question the user asked.
        /// </summary>
        [ExposeToXslt]
        public string Question { get; set; }

        /// <summary>
        /// The date the question was submitted e.g. "4 Aug".
        /// </summary>
        [ExposeToXslt]
        public string QuestionDate { get; set; }

        /// <summary>
        /// The reason the question was rejected.
        /// </summary>
        [ExposeToXslt]
        public string ReasonHtml { get; set; }
    }

}