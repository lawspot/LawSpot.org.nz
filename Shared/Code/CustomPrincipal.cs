using System;
using System.Security;
using System.Security.Principal;
using System.Web.Security;
using Lawspot.Backend;

namespace Lawspot.Shared
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
            result.Id = user.UserId;
            result.EmailAddress = user.EmailAddress;
            result.IsVolunteerAdmin = user.IsVolunteerAdmin;
            result.IsLawyer = user.IsRegisteredLawyer && user.Lawyer.Approved;
            result.IsCLCLawyer = user.IsCLCLawyer;
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
            int identifierLength = ticket.UserData.Length;
            for (int i = 0; i < ticket.UserData.Length; i++)
                if (ticket.UserData[i] < '0' || ticket.UserData[i] > '9')
                {
                    identifierLength = i;
                    break;
                }
            if (identifierLength == 0)
                return null;    // Old format cookie.
            result.Id = int.Parse(ticket.UserData.Substring(0, identifierLength));
            result.EmailAddress = ticket.Name;
            result.IsVolunteerAdmin = ticket.UserData.Contains("V");
            result.IsLawyer = ticket.UserData.Contains("L");
            result.IsCLCLawyer = ticket.UserData.Contains("C");
            return result;
        }

        /// <summary>
        /// Constructs a CustomPrincipal instance.
        /// </summary>
        private CustomPrincipal()
        {
        }

        /// <summary>
        /// The ID of the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The user is an approved lawyer.
        /// </summary>
        public bool IsLawyer { get; set; }

        /// <summary>
        /// The user is a community law centre lawyer.
        /// </summary>
        public bool IsCLCLawyer { get; set; }

        /// <summary>
        /// The user is a volunteer that vets and recategorizes questions.
        /// </summary>
        public bool IsVolunteerAdmin { get; set; }

        /// <summary>
        /// Creates a new forms authentication ticket using the information in this principal.
        /// </summary>
        /// <param name="persistant"> <c>true</c> if the ticket will be stored in a persistant
        /// cookie, <c>false</c> otherwise. </param>
        /// <returns></returns>
        public FormsAuthenticationTicket ToTicket(bool persistant)
        {
            var userData = new System.Text.StringBuilder();
            userData.Append(this.Id.ToString());
            if (this.IsLawyer)
                userData.Append("L");
            if (this.IsCLCLawyer)
                userData.Append("C");
            if (this.IsVolunteerAdmin)
                userData.Append("V");
            return new FormsAuthenticationTicket(1, this.EmailAddress, DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout), persistant,
                userData.ToString(), FormsAuthentication.FormsCookiePath);
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