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
        /// The user's full name, or <c>null</c> if we don't have it.
        /// </summary>
        public string FullName
        {
            get
            {
                if (this.FirstName == null && this.LastName == null)
                    return null;
                return string.Format("{0} {1}", this.FirstName, this.LastName).Trim();
            }
        }

        /// <summary>
        /// Gets the user's first name, if they are a lawyer, or "there" otherwise.
        /// </summary>
        public string EmailGreeting
        {
            get
            {
                if (FirstName == null)
                    return "there";
                return this.FirstName;
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
                if (this.FullName == null)
                    return this.EmailAddress;
                return string.Format("{0} <{1}>", this.FullName, this.EmailAddress);
            }
        }

        /// <summary>
        /// Gets the user's full name, if we have it, or just their email address otherwise.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (this.FullName == null)
                    return this.EmailAddress;
                return FullName;
            }
        }
    }
}