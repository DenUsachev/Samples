using System;
using Newtonsoft.Json;

namespace Samshit.AuthGateway.Models
{
    [JsonObject]
    public class ErrorData
    {
        public ErrorData(string errorText, Guid traceId)
        {
            ErrorText = errorText;
            TraceId = traceId;
        }

        public string ErrorText { get; set; }
        public Guid TraceId { get; set; }
    }
}

