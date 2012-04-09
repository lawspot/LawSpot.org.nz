using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    public partial class User
    {
        /// <summary>
        /// Gets a value that indicates whether a user is a registered lawyer.
        /// </summary>
        public bool IsRegisteredLawyer
        {
            get { return this.Lawyers.Any(); }
        }

        /// <summary>
        /// Gets the lawyer details for a user.
        /// </summary>
        public Lawyer Lawyer
        {
            get
            {
                var lawyer = this.Lawyers.SingleOrDefault();
                if (lawyer == null)
                    throw new InvalidOperationException(string.Format("The user {0} is not a lawyer.", this.EmailAddress));
                return lawyer;
            }
        }
    }
}