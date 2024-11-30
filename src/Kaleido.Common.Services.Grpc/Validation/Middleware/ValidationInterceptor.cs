using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Reflection;
using Kaleido.Common.Services.Grpc.Validation.Attributes;

namespace Kaleido.Common.Services.Grpc.Validation.Middleware;

public class ValidationInterceptor : Interceptor
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var methodInfo = context.Method.Split('/').Last();
        var serviceType = continuation.Target?.GetType();
        var method = serviceType?.GetMethods()
            .FirstOrDefault(m => m.Name == methodInfo);

        if (method != null)
        {
            var validationAttributes = method.GetCustomAttributes()
                .Where(attr => attr.GetType().IsGenericType &&
                              attr.GetType().GetGenericTypeDefinition() == typeof(ValidationAttribute<>));

            foreach (dynamic validationAttribute in validationAttributes)
            {
                await validationAttribute.ValidateAsync(request, _serviceProvider);
            }
        }

        return await continuation(request, context);
    }
}