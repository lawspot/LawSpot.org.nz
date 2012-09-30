using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Lawspot.Shared
{
    public static class StringUtilities
    {
        private static Regex urlRegex = new Regex(@"http(?:s)?://[A-Za-z0-9-._~:/?#\[\]@!$&'()*+,;=]*[A-Za-z0-9-_~:/?#\[@!$&'(*+,;=]", RegexOptions.Compiled);

        /// <summary>
        /// Converts plain text into HTML, with line breaks and hyperlinks.
        /// </summary>
        /// <param name="text"> The text to convert. </param>
        /// <returns> An HTML representation of the text. </returns>
        public static string ConvertTextToHtml(string text)
        {
            if (text == null)
                return string.Empty;

            // Newlines should be converted to <br> tags.
            var result = new StringBuilder();
            int start = 0;
            while (start < text.Length)
            {
                // Find the newline or linefeed.
                int newLineIndex = text.IndexOf(Environment.NewLine, start);
                newLineIndex = newLineIndex < 0 ? text.Length : newLineIndex;
                int lineFeedIndex = text.IndexOf('\n', start);
                lineFeedIndex = lineFeedIndex < 0 ? text.Length : lineFeedIndex;
                
                // Find the next URL.
                Match match = urlRegex.Match(text, start);
                Uri url = null;
                int urlIndex = text.Length;
                if (match.Success && Uri.TryCreate(match.Value, UriKind.Absolute, out url))
                    urlIndex = match.Index;

                // Encode the text.
                int nextEventIndex = Math.Min(Math.Min(newLineIndex, lineFeedIndex), urlIndex);
                result.Append(WebUtility.HtmlEncode(text.Substring(start, nextEventIndex - start)));
                start = nextEventIndex;

                if (newLineIndex < text.Length && newLineIndex < lineFeedIndex && newLineIndex < urlIndex)
                {
                    // Convert newline to <br>.
                    result.AppendLine("<br>");
                    start += 2;
                }
                else if (lineFeedIndex < text.Length && lineFeedIndex < newLineIndex && lineFeedIndex < urlIndex)
                {
                    // Convert linefeed to <br>.
                    result.AppendLine("<br>");
                    start += 1;
                }
                else if (urlIndex < text.Length && urlIndex < newLineIndex && urlIndex < lineFeedIndex)
                {
                    // Convert hyperlink to <a href="http://www.example.com" rel="nofollow">www.example.com</a>.
                    result.AppendFormat(@"<a href=""{0}"" rel=""nofollow"">{1}</a>", url, WebUtility.HtmlEncode(url.Host));
                    start += match.Length;
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Summarizes text longer than a certain number of characters.
        /// </summary>
        /// <param name="maxLength"> The maximum number of characters to return. </param>
        /// <returns> Summarized text. </returns>
        public static string SummarizeText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length <= maxLength)
                return text;
            text = text.Substring(0, maxLength - 3);
            int spaceIndex = text.LastIndexOf(' ');
            if (spaceIndex > 0)
                text = text.Substring(0, spaceIndex);
            text += "...";
            return text;
        }

        /// <summary>
        /// Utility method to set a query string parameter in a URL.
        /// </summary>
        /// <param name="uri"> The URL to change. </param>
        /// <param name="key"> The parameter key. </param>
        /// <param name="value"> The parameter value. </param>
        /// <returns> The provided URL, with the given parameter set to the given value. </returns>
        public static Uri SetUriParameter(Uri uri, string key, object value)
        {
            var builder = new UriBuilder(uri);
            var parameters = System.Web.HttpUtility.ParseQueryString(builder.Query);
            parameters[key] = value.ToString();
            builder.Query = parameters.ToString();
            return builder.Uri;
        }

        /// <summary>
        /// Converts a time-span into a relative time string.
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string ConvertToRelativeTime(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes < 1.0)
                return "less than a minute ago";
            if (timeSpan.TotalMinutes < 1.5)
                return string.Format("1 minute ago", Math.Round(timeSpan.TotalMinutes));
            if (timeSpan.TotalMinutes < 60.0)
                return string.Format("{0} minutes ago", Math.Round(timeSpan.TotalMinutes));
            if (timeSpan.TotalMinutes < 90.0)
                return string.Format("1 hour ago", Math.Round(timeSpan.TotalMinutes));
            if (timeSpan.TotalHours < 24.0)
                return string.Format("{0} hours ago", Math.Round(timeSpan.TotalHours));
            if (timeSpan.TotalHours < 36.0)
                return string.Format("1 day ago", Math.Round(timeSpan.TotalHours));
            return string.Format("{0} days ago", Math.Round(timeSpan.TotalDays));
        }

        /// <summary>
        /// Creates a random string of the given length.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CreateRandomToken(int length)
        {
            var token = new System.Text.StringBuilder();
            var random = new System.Security.Cryptography.RNGCryptoServiceProvider();
            var randomBytes = new byte[length];
            random.GetBytes(randomBytes);
            // Note: this does not produce an even distribution, but it should be good enough
            // for our purposes.
            for (int i = 0; i < length; i++)
                token.Append((char)((int)'A' + (randomBytes[i] % 26)));
            return token.ToString();
        }
    }
}