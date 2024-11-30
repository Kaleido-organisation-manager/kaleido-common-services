using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Validation.Middleware;

public class ValidationMiddleware<TValidator> : Interceptor
    where TValidator : class, IValidator
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationMiddleware(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var validator = _serviceProvider.GetRequiredService<TValidator>();

        if (validator is IValidator<TRequest> requestValidator)
        {
            await requestValidator.ValidateAndThrowAsync(request);
        }

        return await continuation(request, context);
    }
}