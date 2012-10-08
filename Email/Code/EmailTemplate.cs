using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Lawspot.Email
{

    /// <summary>
    /// The base class of all email templates.
    /// </summary>
    public abstract class EmailTemplate : MailMessage
    {
        public EmailTemplate()
        {
            this.BaseUrl = "http://" + System.Configuration.ConfigurationManager.AppSettings["DomainName"];
            this.CurrentDate = DateTime.Now.ToString("dddd, d MMMM yyyy");
        }

        /// <summary>
        /// The path to the XSLT template file, relative to the Templates directory.
        /// </summary>
        protected string TemplateFilePath { get; set; }

        /// <summary>
        /// The base URL for images, e.g. http://www.lawspot.org.nz
        /// </summary>
        [ExposeToXsltAttribute]
        public string BaseUrl { get; private set; }

        /// <summary>
        /// Today's date e.g. "Friday, 6 April 2012".
        /// </summary>
        [ExposeToXsltAttribute]
        public string CurrentDate { get; private set; }

        /// <summary>
        /// Sends the message.
        /// </summary>
        public void Send()
        {
            // Create an XML document with the needed info.
            var doc = new XmlDocument();
            var root = doc.CreateElement("Root");
            doc.AppendChild(root);
            SerializeToXml(this, root);

            // Determine the URI of the XSLT.
            if (this.TemplateFilePath == null)
                throw new InvalidOperationException("TemplateFilePath must be specified.");
            Uri templateUri = new Uri(Path.Combine(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"Email\Templates"), this.TemplateFilePath));

            // Create an XML writer to contain the output.
            var output = new StringBuilder();
            var xmlWriter = new XmlTextWriter(new StringWriter(output));

            // Process the XSLT.
            var xsltProcessor = new XslCompiledTransform();
            xsltProcessor.Load(templateUri.AbsoluteUri);
            xsltProcessor.Transform(doc, xmlWriter);
            
            // Set the body of the email.
            this.IsBodyHtml = true;
            this.Body = output.ToString();

            // Send the email.
            using (var client = new SmtpClient())
            {
                client.Send(this);
            }
        }

        private void SerializeToXml(object value, XmlElement parent)
        {
            if (value == null)
                return;
            var valueType = value.GetType();
            if (Type.GetTypeCode(valueType) != TypeCode.Object)
            {
                // Primitive value.
                parent.InnerText = value.ToString();
            }
            else if (value is System.Collections.IEnumerable)
            {
                // Collection.
                foreach (var item in (System.Collections.IEnumerable)value)
                {
                    if (item == null)
                        continue;
                    var itemType = item.GetType();
                    var element = parent.OwnerDocument.CreateElement(itemType.Name);
                    parent.AppendChild(element);
                    SerializeToXml(item, element);
                }
            }
            else
            {
                // Object.
                foreach (var property in valueType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    // Make sure the property has a [ExposeToXsltAttribute] attribute.
                    var attribute = (ExposeToXsltAttribute)Attribute.GetCustomAttribute(property, typeof(ExposeToXsltAttribute));
                    if (attribute == null)
                        continue;

                    var element = parent.OwnerDocument.CreateElement(property.Name);
                    var propertyValue = property.GetValue(value, null);
                    SerializeToXml(propertyValue, element);
                    parent.AppendChild(element);
                }
            }
        }
    }

}