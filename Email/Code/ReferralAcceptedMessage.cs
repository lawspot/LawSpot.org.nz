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
    /// The email sent to a lawyer when they accept a referral.
    /// </summary>
    public class ReferralAcceptedMessage : EmailTemplate
    {
        public ReferralAcceptedMessage()
        {
            this.TemplateFilePath = "ReferralAccepted.xslt";
            this.Subject = "Details of your referral";
        }

        /// <summary>
        /// The name of the client.
        /// </summary>
        [ExposeToXslt]
        public string ClientName { get; set; }

        /// <summary>
        /// The email address of the client.
        /// </summary>
        [ExposeToXslt]
        public string ClientEmail { get; set; }

        /// <summary>
        /// The phone number of the client.
        /// </summary>
        [ExposeToXslt]
        public string ClientPhone { get; set; }

        /// <summary>
        /// The location of the client.
        /// </summary>
        [ExposeToXslt]
        public string ClientLocation { get; set; }

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
    }

}