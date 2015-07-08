using System;
using System.Collections.Generic;
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
    public class QuestionReferredMessage : EmailTemplate
    {
        public QuestionReferredMessage()
        {
            this.TemplateFilePath = "QuestionReferred.xslt";
            this.Subject = "Your question can be referred to a partner lawyer";
            this.ReferralPartners = new List<ReferralPartner>();
        }

        /// <summary>
        /// The ID of the question that was referred.
        /// </summary>
        [ExposeToXslt]
        public List<ReferralPartner> ReferralPartners { get; set; }
    }

    public class ReferralPartner
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUri { get; set; }
        public string LinkUri { get; set; }
    }
}