using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using Lawspot.Email;

namespace Lawspot.Shared
{

    /// <summary>
    /// Logs errors.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log an exception as an error.
        /// </summary>
        /// <param name="exception"> The exception to log. </param>
        public static void LogError(Exception exception)
        {
            LogError(exception, exception.Message);
        }

        /// <summary>
        /// Log a message as an error.
        /// </summary>
        /// <param name="message"> The message to log. </param>
        /// <param name="args"> Message arguments. </param>
        public static void LogError(string message, params object[] args)
        {
            LogError(null, message, args);
        }

        /// <summary>
        /// Log an exception with a custom message.
        /// </summary>
        /// <param name="exception"> The exception to log. </param>
        /// <param name="message"> The message to log. </param>
        /// <param name="args"> Message arguments. </param>
        public static void LogError(Exception exception, string message, params object[] args)
        {
            LogError(exception, string.Format(message, args));
        }

        /// <summary>
        /// Log an exception with a custom message.
        /// </summary>
        /// <param name="exception"> The exception to log. </param>
        /// <param name="message"> The message to log. </param>
        public static void LogError(Exception exception, string message)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            var email = new ErrorTemplate();
            email.Application = ConfigurationManager.AppSettings["DomainName"];
            email.To.Add(ConfigurationManager.AppSettings["SendErrorEmailTo"]);

            // If the error message contains a newline, truncate the title there.
            if (message.Contains(Environment.NewLine))
                email.Subject = "LawSpot error: " + message.Substring(0, message.IndexOf(Environment.NewLine));
            else
                email.Subject = "LawSpot error: " + message;

            email.ErrorMessage = message;
            email.StackTrace = exception == null ? null : exception.ToString();
            if (exception != null)
            {
                foreach (var key in exception.Data.Keys)
                    email.ExceptionData.Add(new ErrorTemplate.NameValuePair() { Name = key.ToString(), Value = exception.Data[key].ToString() });
            }
            email.ServerHost = string.Format("{0}.{1}", Environment.MachineName,
                    System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName);;
            email.ServerCommandLine = Environment.CommandLine;
            email.CurrentTime = DateTime.Now.ToLocalTime().ToString("d/M/yyyy hh:mm:ss.fff tt");

            // Pull out the HTTP information.
            if (HttpContext.Current != null)
            {
                var context = HttpContext.Current;
                var request = context.Request;
                email.RequestTime = context.Timestamp.ToString();
                email.RequestType = request.HttpMethod;
                email.RequestUrl = request.Url.ToString();
                email.RequestReferrer = request.UrlReferrer != null ? request.UrlReferrer.ToString() : null;
                email.RequestLength = request.ContentLength.ToString();
                email.UserRawIP = request.UserHostAddress;
                if (request.Headers["X-Forwarded-For"] != null)
                {
                    // Note that X-Forwarded-For can contain multiple addresses - one for each proxy.
                    // The left-most address is closest to the client.  Ideally we would take the
                    // left-most address that is not a local address (192.168.x.x or 10.x.x.x) but
                    // taking the right-most address is simpler for now.
                    email.UserIP = request.Headers["X-Forwarded-For"];
                    var commaIndex = email.UserIP.TrimEnd(',').LastIndexOf(',');
                    if (commaIndex >= 0)
                        email.UserIP = email.UserIP.Substring(commaIndex + 1).Trim();
                }
                else
                    email.UserIP = request.UserHostAddress;
                email.UserAgent = request.UserAgent;
                if (request.Form != null && request.Form.Count > 0)
                {
                    var formData = new System.Text.StringBuilder();
                    for (int i = 0; i < request.Form.Count; i++)
                    {
                        if (formData.Length > 0)
                            formData.Append('&');

                        // Blacklist keys containing sensitive data.
                        var name = request.Form.Keys[i];
                        var value = request.Form[i] ?? string.Empty;
                        bool isBlacklisted =
                            name.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0;

                        email.RequestFormData.Add(new ErrorTemplate.NameValuePair()
                        {
                            Name = name,
                            Value = isBlacklisted && value.Length > 0 ? "<blocked>" : value
                        });
                    }
                }
                else if (request.ContentType == "application/json")
                {
                    request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    email.RequestData = new System.IO.StreamReader(request.InputStream).ReadToEnd();
                }
                email.RequestLength = string.Format("{0} bytes", request.ContentLength);
                email.ContentType = request.ContentType;
                email.ContentEncoding = request.ContentEncoding.WebName;
                for (int i = 0; i < request.Cookies.Count; i++)
                {
                    email.RequestCookies.Add(new ErrorTemplate.NameValuePair()
                    {
                        Name = request.Cookies[i].Name,
                        Value = request.Cookies[i].Value
                    });
                }
                email.UserLanguages = request.UserLanguages == null ? null : string.Join(", ", request.UserLanguages);
                if (context.User is CustomPrincipal)
                    email.UserName = ((CustomPrincipal)context.User).EmailAddress;
            }

            // Send the email.
            email.Send();
        }
    }

}