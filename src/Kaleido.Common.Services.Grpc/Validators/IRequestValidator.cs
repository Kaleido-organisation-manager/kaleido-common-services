using Kaleido.Common.Services.Grpc.Models.Validations;

namespace Kaleido.Common.Services.Grpc.Validators;

public interface IRequestValidator<T>
{
    Task<ValidationResult> ValidateAsync(T request, CancellationToken cancellationToken = default);
}
