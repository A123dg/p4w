namespace p4w.Core.Exceptions;

public sealed class AppException : Exception
{
    public int ErrorCode { get; }
    public int StatusCode { get; }

    public AppException(string message, int errorCode = ErrorCodes.BadRequest, int statusCode = 400) : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}
