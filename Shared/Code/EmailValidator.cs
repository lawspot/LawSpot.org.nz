using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lawspot.Shared
{
    public class EmailValidator
    {
        /// <summary>
        /// Validates the domain name part of the email address.
        /// </summary>
        /// <param name="email"> The email address to validate. </param>
        /// <returns> <c>true</c> if the email address is okay; <c>false</c> otherwise. </returns>
        public static bool ValidateEmailDomain(string email)
        {
            email = email.Trim();
            try
            {
                var mxRecords = JHSoftware.DnsClient.LookupMX(email.Substring(email.IndexOf('@') + 1));
                return mxRecords.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}