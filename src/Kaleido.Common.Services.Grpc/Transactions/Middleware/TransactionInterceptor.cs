using Grpc.Core;
using Grpc.Core.Interceptors;
using Kaleido.Common.Services.Grpc.Transactions.Attributes;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Kaleido.Common.Services.Grpc.Transactions.Middleware;

public class TransactionInterceptor : Interceptor
{
    private readonly IEnumerable<DbContext> _contexts;

    public TransactionInterceptor(IEnumerable<DbContext> contexts)
    {
        _contexts = contexts;
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

        if (method?.GetCustomAttribute<TransactionAttribute>() != null)
        {
            var transactionAttr = new TransactionAttribute();
            TResponse response = default!;

            await transactionAttr.ExecuteInTransactionAsync(_contexts, async () =>
            {
                response = await continuation(request, context);
            });

            return response;
        }

        return await continuation(request, context);
    }
}