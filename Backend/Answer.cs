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
    }
}