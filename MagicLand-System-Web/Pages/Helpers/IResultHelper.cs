namespace MagicLand_System_Web.Pages.Helper
{
    public interface IResultHelper<T>
    {
        bool IsSuccess { get; }
        T Data { get; }
        string Message { get; }
        string StatusCode { get; }
    }
}
