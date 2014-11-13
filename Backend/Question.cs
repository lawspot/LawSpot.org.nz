using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    public enum QuestionStatus
    {
        Unreviewed = 0,
        Approved = 1,
        Rejected = 2,
        Referral = 3,
        AcceptedReferral = 4,
    }

    public partial class Question
    {
        /// <summary>
        /// Gets the path of the question page.
        /// </summary>
        public string AbsolutePath
        {
            get { return string.Format("/{0}/{1}", this.Category.Slug, this.Slug); }
        }

        /// <summary>
        /// Gets the full URI to the question page.
        /// </summary>
        public string Uri
        {
            get { return string.Format("http://{0}{1}", ConfigurationManager.AppSettings["DomainName"], AbsolutePath); }
        }

        /// <summary>
        /// Gets the full URI to the answer question page in admin.
        /// </summary>
        public string AdminUri
        {
            get { return string.Format("http://{0}/admin/answer-question?questionId={1}", ConfigurationManager.AppSettings["DomainName"], this.QuestionId); }
        }
    }
}