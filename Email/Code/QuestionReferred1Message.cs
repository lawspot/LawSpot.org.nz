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
    /// The email sent to a user when a question is placed in the referral queue by a reviewer.
    /// </summary>
    public class QuestionReferred1Message : EmailTemplate
    {
        public QuestionReferred1Message()
        {
            this.TemplateFilePath = "QuestionReferred1.xslt";
            this.Subject = "Your question can be referred to a partner lawyer";
        }

        /// <summary>
        /// The ID of the question that was referred.
        /// </summary>
        [ExposeToXslt]
        public int QuestionId { get; set; }
    }

}