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

        /// <summary>
        /// Gets the user's first name, if they are a lawyer, or "there" otherwise.
        /// </summary>
        public string EmailGreeting
        {
            get
            {
                var lawyer = this.Lawyers.SingleOrDefault();
                if (lawyer == null)
                    return "there";
                return lawyer.FirstName;
            }
        }

        /// <summary>
        /// Gets the user's full name + email address, if they are a lawyer, or just their email
        /// address otherwise, in the form "Full Name &lt;email@domain.com&gt;".
        /// </summary>
        public string EmailDisplayName
        {
            get
            {
                var lawyer = this.Lawyers.SingleOrDefault();
                if (lawyer == null)
                    return this.EmailAddress;
                return string.Format("{0} <{1}>", lawyer.FullName, this.EmailAddress);
            }
        }
    }
}