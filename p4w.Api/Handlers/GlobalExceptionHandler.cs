using Microsoft.AspNetCore.Diagnostics;
using p4w.Core.Constants;
using p4w.Core.Exceptions;
using p4w.Core.Paginations;

namespace p4w.Api.Handlers;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");

        var status = StatusCodes.Status500InternalServerError;
        var code = ErrorCodes.InternalServerError;
        var message = MessageConstant.CommonMessage.INTERNAL_SERVER_ERROR;

        switch (exception)
        {
            case AppException appException:
                status = appException.StatusCode;
                code = appException.ErrorCode;
                message = appException.Message;
                break;
            case UnauthorizedAccessException:
                status = StatusCodes.Status401Unauthorized;
                code = ErrorCodes.Unauthorized;
                message = MessageConstant.CommonMessage.UNAUTHORIZED;
                break;
            case BadHttpRequestException:
                status = StatusCodes.Status400BadRequest;
                code = ErrorCodes.BadRequest;
                message = MessageConstant.CommonMessage.MISSING_PARAM;
                break;
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ApiResponse<object>
        {
            Code = code,
            Success = false,
            Message = message,
            Data = null,
            MetaData = null
        }, cancellationToken);
        return true;
    }
}
