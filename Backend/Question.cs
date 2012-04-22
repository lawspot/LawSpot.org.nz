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
    }
}