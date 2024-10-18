using Grpc.Core;
using Kaleido.Common.Services.Grpc.Models.Validations;

namespace Kaleido.Common.Services.Grpc.Exceptions;

public class ValidationException : RpcException
{
    public IEnumerable<ValidationError> Errors { get; }
    public ValidationException(string message, IEnumerable<ValidationError> errors) : base(new Status(StatusCode.InvalidArgument, message))
    {
        Errors = errors;
    }
}