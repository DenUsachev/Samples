using System;

namespace Samshit.WebUtils
{
    public class ErrorData
    {
        public ErrorData(string errorText, Guid traceId, object details = null)
        {
            ErrorText = errorText;
            TraceId = traceId;
            ErrorDetails = details;
        }

        public string ErrorText { get; }
        public object ErrorDetails { get; }
        public Guid TraceId { get; }
    }
}