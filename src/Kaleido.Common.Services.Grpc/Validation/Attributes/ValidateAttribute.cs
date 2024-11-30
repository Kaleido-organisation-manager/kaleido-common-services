using System.Reflection;
using FluentValidation;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Kaleido.Common.Services.Grpc.Validation.Middleware;

namespace Kaleido.Common.Services.Grpc.Validation.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateAttribute<TValidator> : Attribute where TValidator : class, IValidator
{
    public static Interceptor CreateInterceptor(IServiceProvider serviceProvider)
    {
        return new ValidationMiddleware<TValidator>(serviceProvider);
    }
}