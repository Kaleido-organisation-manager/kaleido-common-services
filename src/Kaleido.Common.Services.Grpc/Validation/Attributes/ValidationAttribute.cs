using FluentValidation;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Kaleido.Common.Services.Grpc.Validation.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ValidationAttribute<TValidator> : Attribute where TValidator : class, IValidator
{
    public async Task ValidateAsync(object request, IServiceProvider serviceProvider)
    {
        var validator = (IValidator)ActivatorUtilities.CreateInstance(serviceProvider, typeof(TValidator));

        var validationContext = new ValidationContext<object>(request);
        var validationResult = await validator.ValidateAsync(validationContext);

        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new RpcException(new Status(StatusCode.InvalidArgument, $"Validation failed: {errors}"));
        }
    }
}