using FluentValidation;
using FluentValidation.Results;
using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions;
using Microsoft.AspNetCore.Connections;

namespace Kaleido.Modules.Services.Grpc.Categories.Common.Helpers;

public static class ExceptionMapper
{
    public static RpcException MapToRpcException(Exception exception, string context)
    {
        return exception switch
        {
            ValidationException validationEx => new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    $"Validation failed for {context}: {string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage))}",
                    validationEx)),

            RevisionNotFoundException => new RpcException(
                new Status(
                    StatusCode.NotFound,
                    $"Entity not found for {context}",
                    exception)),

            EntityNotFoundException => new RpcException(
                new Status(
                    StatusCode.NotFound,
                    $"Entity not found for {context}",
                    exception)),

            NotModifiedException => new RpcException(
                new Status(
                    StatusCode.AlreadyExists,
                    $"Entity already exists for {context}: {exception.Message}",
                    exception)),

            InvalidOperationException => new RpcException(
                new Status(
                    StatusCode.FailedPrecondition,
                    $"Invalid operation for {context}: {exception.Message}",
                    exception)),

            ArgumentException => new RpcException(
                new Status(
                    StatusCode.InvalidArgument,
                    $"Invalid argument for {context}: {exception.Message}",
                    exception)),

            OperationCanceledException => new RpcException(
                new Status(
                    StatusCode.Cancelled,
                    $"Operation cancelled for {context}",
                    exception)),

            _ => new RpcException(
                new Status(
                    StatusCode.Internal,
                    $"An unexpected error occurred while processing {context}",
                    exception))
        };
    }

    public static RpcException MapToRpcException(Exception exception)
    {
        return MapToRpcException(exception, "request");
    }

    public static Exception MapFromRpcException(RpcException exception)
    {
        return exception.Status.StatusCode switch
        {
            StatusCode.InvalidArgument => new ValidationException(
                "Validation failed: " + exception.Status.Detail,
                Enumerable.Empty<ValidationFailure>()),

            StatusCode.NotFound => new NotFoundException(
                exception.Status.Detail ?? "The requested entity was not found"),

            StatusCode.AlreadyExists => new AlreadyExistsException(
                exception.Status.Detail ?? "The entity already exists"),

            StatusCode.FailedPrecondition => new InvalidOperationException(
                exception.Status.Detail ?? "The operation cannot be performed in the current state"),

            StatusCode.Cancelled => new OperationCanceledException(
                exception.Status.Detail ?? "The operation was cancelled"),

            StatusCode.DeadlineExceeded => new TimeoutException(
                exception.Status.Detail ?? "The operation timed out"),

            StatusCode.PermissionDenied => new UnauthorizedAccessException(
                exception.Status.Detail ?? "Permission denied for this operation"),

            StatusCode.Aborted => new ConnectionAbortedException(
                exception.Status.Detail ?? "The connection was aborted"),

            _ => new Exception(
                exception.Status.Detail ?? $"An error occurred with status: {exception.Status.StatusCode}")
        };
    }
}