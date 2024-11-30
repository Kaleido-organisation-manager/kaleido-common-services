using Kaleido.Common.Services.Grpc.Exceptions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Kaleido.Common.Services.Grpc.Exceptions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IApplicationBuilder UseGrpcExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}