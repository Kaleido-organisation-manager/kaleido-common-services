using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions.Helpers;
using Microsoft.AspNetCore.Http;

namespace Kaleido.Common.Services.Grpc.Exceptions.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RpcException)
        {
            // RpcExceptions are already properly formatted, just rethrow
            throw;
        }
        catch (Exception ex)
        {
            // Map the exception to an RpcException and throw
            throw ExceptionMapper.MapToRpcException(ex, context.Request.Path);
        }
    }
}