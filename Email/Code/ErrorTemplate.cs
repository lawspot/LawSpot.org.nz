using System;
using System.Collections.Generic;

namespace Lawspot.Email
{
    /// <summary>
    /// Used when emailing exception details to an admin.
    /// </summary>
    public class ErrorTemplate : EmailTemplate
    {
        /// <summary>
        /// Creates a new ErrorEmail instance.
        /// </summary>
        public ErrorTemplate()
        {
            this.TemplateFilePath = "Error.xslt";
            this.RequestTime = "N/A";
            this.RequestType = "N/A";
            this.RequestUrl = "N/A";
            this.RequestReferrer = "N/A";
            this.RequestLength = "N/A";
            this.ContentType = "N/A";
            this.ContentEncoding = "N/A";
            this.UserRawIP = "N/A";
            this.UserIP = "N/A";
            this.UserAgent = "N/A";
            this.ClientApplication = "N/A";
            this.TrueIdentity = "N/A";
            this.RequestData = "N/A";
            this.RequestLength = "N/A";
            this.UserLanguages = "N/A";
            this.UserName = "N/A";
            this.ExceptionData = new List<NameValuePair>();
            this.RequestCookies = new List<NameValuePair>();
            this.RequestFormData = new List<NameValuePair>();
        }

        /// <summary>
        /// The error message.
        /// </summary>
        [ExposeToXslt]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        [ExposeToXslt]
        public string ErrorMessageHtml
        {
            get
            {
                return System.Net.WebUtility.HtmlEncode(this.ErrorMessage).Replace(Environment.NewLine, "<br />");
            }
            set { /* Required by XmlSerializer */ }
        }

        /// <summary>
        /// The stack trace at the point the error was generated.
        /// </summary>
        [ExposeToXslt]
        public string StackTrace { get; set; }

        /// <summary>
        /// The contents of the exception.Data property.
        /// </summary>
        [ExposeToXslt]
        public List<NameValuePair> ExceptionData { get; set; }

        /// <summary>
        /// The time the HTTP request was made.
        /// </summary>
        [ExposeToXslt]
        public string RequestTime { get; set; }

        /// <summary>
        /// The time this error report was generated.
        /// </summary>
        [ExposeToXslt]
        public string CurrentTime { get; set; }

        /// <summary>
        /// The HTTP method (GET, POST, etc).
        /// </summary>
        [ExposeToXslt]
        public string RequestType { get; set; }

        /// <summary>
        /// The URL of the page that was requested.
        /// </summary>
        [ExposeToXslt]
        public string RequestUrl { get; set; }
        
        /// <summary>
        /// The HTTP referrer.
        /// </summary>
        [ExposeToXslt]
        public string RequestReferrer { get; set; }

        /// <summary>
        /// The length of the request body, in bytes.
        /// </summary>
        [ExposeToXslt]
        public string RequestLength { get; set; }

        /// <summary>
        /// The MIME type of the request body.
        /// </summary>
        [ExposeToXslt]
        public string ContentType { get; set; }

        /// <summary>
        /// The encoding of the request body.
        /// </summary>
        [ExposeToXslt]
        public string ContentEncoding { get; set; }

        /// <summary>
        /// The cookies associated with the request.
        /// </summary>
        [ExposeToXslt]
        public List<NameValuePair> RequestCookies { get; set; }

        /// <summary>
        /// Request data, as a string.
        /// </summary>
        [ExposeToXslt]
        public string RequestData { get; set; }

        /// <summary>
        /// A name/value pair.
        /// </summary>
        public class NameValuePair
        {
            /// <summary>
            /// The name.
            /// </summary>
            [ExposeToXslt]
            public string Name { get; set; }

            /// <summary>
            /// The value.
            /// </summary>
            [ExposeToXslt]
            public string Value { get; set; }
        }

        /// <summary>
        /// Form data, expressed as key/value pairs.
        /// </summary>
        [ExposeToXslt]
        public List<NameValuePair> RequestFormData { get; set; }

        /// <summary>
        /// The IP of the user.
        /// </summary>
        [ExposeToXslt]
        public string UserIP { get; set; }

        /// <summary>
        /// The username of the user, if available.
        /// </summary>
        [ExposeToXslt]
        public string UserName { get; set; }

        /// <summary>
        /// The username of the impersonating user, if available.
        /// </summary>
        [ExposeToXslt]
        public string TrueIdentity { get; set; }

        /// <summary>
        /// The name of the application the client is using to access the API.
        /// </summary>
        [ExposeToXslt]
        public string ClientApplication { get; set; }

        /// <summary>
        /// The user's browser.
        /// </summary>
        [ExposeToXslt]
        public string UserAgent { get; set; }

        /// <summary>
        /// The raw IP address for the HTTP connection.
        /// </summary>
        [ExposeToXslt]
        public string UserRawIP { get; set; }

        /// <summary>
        /// The list of languages sent by the user.
        /// </summary>
        [ExposeToXslt]
        public string UserLanguages { get; set; }

        /// <summary>
        /// The name of the application that had the error.
        /// </summary>
        [ExposeToXslt]
        public string Application { get; set; }

        /// <summary>
        /// The name of the server on which the error originated.
        /// </summary>
        [ExposeToXslt]
        public string ServerHost { get; set; }

        /// <summary>
        /// The command line that started the server process.
        /// </summary>
        [ExposeToXslt]
        public string ServerCommandLine { get; set; }
    }
}
