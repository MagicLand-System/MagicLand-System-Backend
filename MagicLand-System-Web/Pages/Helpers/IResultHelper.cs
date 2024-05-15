namespace MagicLand_System_Web_Dev.Pages.Helper
{
    public interface IResultHelper<T>
    {
        bool IsSuccess { get; }
        T Data { get; }
        string Message { get; }
        string StatusCode { get; }
    }
}
