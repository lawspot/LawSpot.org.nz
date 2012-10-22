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
    /// The email sent to a user when an answer for a question of theirs has been approved.
    /// </summary>
    public class QuestionAnsweredMessage : EmailTemplate
    {
        public QuestionAnsweredMessage()
        {
            this.TemplateFilePath = "QuestionAnswered.xslt";
            this.Subject = "Your question has been answered";
        }

        /// <summary>
        /// The title of the question the user asked.
        /// </summary>
        [ExposeToXslt]
        public string Question { get; set; }

        /// <summary>
        /// The question details.
        /// </summary>
        [ExposeToXslt]
        public string DetailsHtml { get; set; }

        /// <summary>
        /// The URL of the dedicated question & answer page.
        /// </summary>
        [ExposeToXslt]
        public string QuestionUri { get; set; }

        /// <summary>
        /// The answer to the question.
        /// </summary>
        [ExposeToXslt]
        public string AnswerHtml { get; set; }
    }

}