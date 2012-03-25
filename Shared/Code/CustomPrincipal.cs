using System;
using System.Security;
using System.Security.Principal;
using System.Web.Security;
using Prolawyers.Backend;

namespace Prolawyers.Shared
{
    public class CustomPrincipal : IPrincipal
    {
        /// <summary>
        /// Populates a CustomPrincipal using a user entity instance.
        /// </summary>
        /// <param name="user"> The user details. </param>
        /// <returns> A CustomPrincipal. </returns>
        public static CustomPrincipal FromUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            var result = new CustomPrincipal();
            result.EmailAddress = user.EmailAddress;
            return result;
        }

        /// <summary>
        /// Populates a CustomPrincipal using an authentication ticket.
        /// </summary>
        /// <param name="user"> The authentication ticket. </param>
        /// <returns> A CustomPrincipal. </returns>
        public static CustomPrincipal FromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");
            var result = new CustomPrincipal();
            result.EmailAddress = ticket.Name;
            return result;
        }

        /// <summary>
        /// Constructs a CustomPrincipal instance.
        /// </summary>
        private CustomPrincipal()
        {
        }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Creates a new forms authentication ticket using the information in this principal.
        /// </summary>
        /// <param name="persistant"> <c>true</c> if the ticket will be stored in a persistant
        /// cookie, <c>false</c> otherwise. </param>
        /// <returns></returns>
        public FormsAuthenticationTicket ToTicket(bool persistant)
        {
            return new FormsAuthenticationTicket(1, this.EmailAddress, DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout), persistant,
                string.Empty, FormsAuthentication.FormsCookiePath);
        }

        #region IPrincipal implementation

        private class CustomIdentity : IIdentity
        {
            private string name;

            public CustomIdentity(string name)
            {
                this.name = name;
            }

            public string AuthenticationType
            {
                get { return "Forms"; }
            }

            public bool IsAuthenticated
            {
                get { return true; }
            }

            public string Name
            {
                get { return this.name; }
            }
        }

        /// <summary>
        /// The identity.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        IIdentity IPrincipal.Identity
        {
            get { return new CustomIdentity(this.EmailAddress); }
        }

        /// <summary>
        /// Determines if the 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool IsInRole(string role)
        {
            return false;
        }

        #endregion
    }
}