using Newtonsoft.Json;
using Samshit.WebUtils.ErrorHandling;

namespace Samshit.WebUtils
{
    [JsonObject]
    public class OperationResult<T> where T : class
    {
        private bool _isSuccessInternal;

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public object ErrorData { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string ErrorCode { get; set; }

        public bool IsSuccess
        {
            get => _isSuccessInternal;

            set
            {
                _isSuccessInternal = value;
                if (value == false)
                    ErrorCode = string.IsNullOrEmpty(ErrorCode) ? ErrorCodes.GenericError : ErrorCode;
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public T Data { get; set; }

        public OperationResult()
        {
            ErrorCode = null;
        }
    }
}