using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    public partial class Lawyer
    {
        public string FullName
        {
            get { return string.Format("{0} {1}", this.FirstName, this.LastName); }
        }
    }
}