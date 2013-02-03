using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    public enum AnswerStatus
    {
        Unreviewed = 0,
        Approved = 1,
        Rejected = 2,
        RecommendedForApproval = 3,
        RequestChanges = 4,
    }

    public partial class Answer
    {
        /// <summary>
        /// A string describing when the answer was published.
        /// </summary>
        public string PublishedText
        {
            get
            {
                var published = this.ReviewDate ?? this.CreatedOn;
                var hoursAgo = (int)Math.Round(DateTimeOffset.Now.Subtract(published).TotalHours);
                if (hoursAgo > 24)
                    return string.Format("{0:d MMM yyyy}", published);
                if (hoursAgo > 1)
                    return string.Format("{0} hours ago", hoursAgo);
                return "less than an hour ago";
            }
        }
    }
}