namespace MagicLand_System_Web.Pages.Helper
{
    public class ResultHelper<T> : IResultHelper<T>
    {
        public bool IsSuccess { get; private set; }
        public T Data { get; private set; }
        public string Message { get; private set; }
        public string StatusCode { get; private set; }

        private ResultHelper(bool isSuccess, T data, string errorMessage, string statusCode)
        {
            IsSuccess = isSuccess;
            Data = data;
            Message = errorMessage;
            StatusCode = statusCode;
        }

        public static ResultHelper<T> Response(T? data, string message, string statusCode, bool isSuccess)
        {
            return new ResultHelper<T>(isSuccess, data, message, statusCode);
        }

        public static ResultHelper<T> DefaultResponse()
        {
            return new ResultHelper<T>(true, default(T), null, null);
        }
    }
}
