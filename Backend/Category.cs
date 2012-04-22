using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    public partial class Category
    {
        /// <summary>
        /// Gets the path of the category page.
        /// </summary>
        public string AbsolutePath
        {
            get { return string.Format("/{0}", this.Slug); }
        }
    }
}