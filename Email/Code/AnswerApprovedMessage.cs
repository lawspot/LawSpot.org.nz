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
    /// The email sent to a lawyer when their answer has been approved.
    /// </summary>
    public class AnswerApprovedMessage : EmailTemplate
    {
        public AnswerApprovedMessage()
        {
            this.TemplateFilePath = "AnswerApproved.xslt";
            this.Subject = "Your answer has been published";
        }

        /// <summary>
        /// The name of the lawyer.
        /// </summary>
        [ExposeToXslt]
        public string Name { get; set; }

        /// <summary>
        /// The title of the question that was anwered.
        /// </summary>
        [ExposeToXslt]
        public string Question { get; set; }

        /// <summary>
        /// The URL of the published answer.
        /// </summary>
        [ExposeToXslt]
        public string QuestionUrl { get; set; }

        /// <summary>
        /// The answer to the question.
        /// </summary>
        [ExposeToXslt]
        public string Answer { get; set; }

        /// <summary>
        /// The number of unanswered questions on the site.
        /// </summary>
        [ExposeToXslt]
        public int UnansweredQuestionCount { get; set; }
    }

}