using Grpc.Core;
using Kaleido.Common.Services.Grpc.Exceptions.Helpers;

namespace Kaleido.Common.Services.Grpc.Client;

public abstract class BaseGrpcClient
{
    protected async Task<T> ExecuteGrpcCallAsync<T>(Func<Task<T>> grpcCall)
    {
        try
        {
            return await grpcCall();
        }
        catch (RpcException ex)
        {
            throw ExceptionMapper.MapFromRpcException(ex);
        }
    }

    protected async Task ExecuteGrpcCallAsync(Func<Task> grpcCall)
    {
        try
        {
            await grpcCall();
        }
        catch (RpcException ex)
        {
            throw ExceptionMapper.MapFromRpcException(ex);
        }
    }
}