using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Lawspot.Backend
{
    public enum EventType
    {
        ApproveQuestion,
        RejectQuestion,
        CreateAnswer,
        PublishAnswer,
        RejectAnswer,
        RecommendAnswer,
        ReferQuestion,
        AcceptReferral,
    }
}