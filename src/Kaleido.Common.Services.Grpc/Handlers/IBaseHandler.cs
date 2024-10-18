using Kaleido.Common.Services.Grpc.Validators;

namespace Kaleido.Common.Services.Grpc.Handlers;

public interface IBaseHandler<TRequest, TResponse>
{
    public IRequestValidator<TRequest> Validator { get; }
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}