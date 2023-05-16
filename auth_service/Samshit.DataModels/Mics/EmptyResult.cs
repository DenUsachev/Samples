namespace Samshit.DataModels.Mics
{
    public class EmptyResult
    {
        public int StatusCode { get; protected set; }
        public string StatusCodeName { get; protected set; }

        public EmptyResult(int statusCode, string statusCodeName)
        {
            StatusCode = statusCode;
            StatusCodeName = statusCodeName;
        }
    }
}
