namespace Kaleido.Common.Services.Grpc.Handlers;

public interface IBaseHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}