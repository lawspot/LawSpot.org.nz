using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
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
            get { return string.Format("http://{0}{1}", System.Configuration.ConfigurationManager.AppSettings["DomainName"], AbsolutePath); }
        }
    }
}