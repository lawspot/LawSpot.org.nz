using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace LawSpot.Email
{

    /// <summary>
    /// The email template for when a user registers.
    /// </summary>
    public class RegisterTemplate : EmailTemplate
    {
        public RegisterTemplate()
        {
            this.TemplateFilePath = "Register.xml";
            this.Subject = "Thanks for registering with LawSpot";
        }
    }

}