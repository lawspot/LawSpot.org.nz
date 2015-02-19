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
    /// The email sent to a user when a question is picked up from the referral queue by a law firm.
    /// </summary>
    public class QuestionReferred2Message : EmailTemplate
    {
        public QuestionReferred2Message()
        {
            this.TemplateFilePath = "QuestionReferred2.xslt";
            this.Subject = "Your question has been referred to a LawSpot partner";
        }

        /// <summary>
        /// The law firm which accepted the question.
        /// </summary>
        [ExposeToXslt]
        public string LawFirm { get; set; }
    }

}