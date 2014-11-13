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
    /// The email sent to a user when an answer is referred to a law firm.
    /// </summary>
    public class QuestionReferredMessage : EmailTemplate
    {
        public QuestionReferredMessage()
        {
            this.TemplateFilePath = "QuestionReferred.xslt";
            this.Subject = "Your question has been referred to a LawSpot partner";
        }

        /// <summary>
        /// The law firm which accepted the question.
        /// </summary>
        [ExposeToXslt]
        public string LawFirm { get; set; }
    }

}