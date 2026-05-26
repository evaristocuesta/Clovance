namespace Clovance.ApiService.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public object[] MessageArgs { get; }
    public Dictionary<string, object?>? Extensions { get; }

    protected AppException(
        string message,
        int statusCode,
        string errorCode,
        Dictionary<string, object?>? extensions = null,
        object[]? messageArgs = null)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Extensions = extensions;
        MessageArgs = messageArgs ?? Array.Empty<object>();
    }
}
