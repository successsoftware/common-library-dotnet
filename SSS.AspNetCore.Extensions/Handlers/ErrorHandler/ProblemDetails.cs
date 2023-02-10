using Newtonsoft.Json;
using System.Collections.Generic;

namespace SSS.AspNetCore.Extensions.Handlers
{
    public class ProblemDetails
    {
        private const string INVALID_ARGUMENT = "invalid_argument";

        public ProblemDetails() { }

        public ProblemDetails(string errorMessage)
        {
            ErrorCode = INVALID_ARGUMENT;
            ErrorMessage = errorMessage;
        }

        public ProblemDetails(IDictionary<string, string[]> errorDetails)
        {
            ErrorCode = INVALID_ARGUMENT;
            ErrorDetails = errorDetails;
        }

        [JsonIgnore]
        public int? Status { get; set; }
        public string TraceId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public IDictionary<string, string[]> ErrorDetails { get; set; } = new Dictionary<string, string[]>();
        public bool ShouldSerializeErrorDetails() => ErrorDetails?.Count > 0;
    }
}
