using Samshit.DataModels.ErrorDefinitions;

namespace Samshit.AuthGateway.Models
{
    public class OperationResult
    {
        public object ErrorData { get; set; }
        public string ErrorCode { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class OperationResult<T> where T : class
    {
        private bool _isSuccessInternal;
        public object ErrorData { get; set; }
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

        public T Data { get; set; }

        public OperationResult()
        {
            ErrorCode = null;
        }

        public static OperationResult<T> CreateOk(T data)
        {
            return new OperationResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static OperationResult<T> CreateFail(string errorCode, object errorData = null)
        {
            return new OperationResult<T>
            {
                IsSuccess = false,
                ErrorData = errorData,
                ErrorCode = errorCode
            };
        }
    }
}