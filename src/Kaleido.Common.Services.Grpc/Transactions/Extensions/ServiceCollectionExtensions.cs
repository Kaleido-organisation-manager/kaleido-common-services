using Grpc.AspNetCore.Server;
using Kaleido.Common.Services.Grpc.Transactions.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Transactions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrpcTransactions(this IServiceCollection services)
    {
        services.AddScoped<TransactionInterceptor>(sp =>
        {
            var contexts = sp.GetServices<DbContext>();
            return new TransactionInterceptor(contexts);
        });

        services.Configure<GrpcServiceOptions>(options =>
        {
            options.Interceptors.Add<TransactionInterceptor>();
        });

        return services;
    }
}