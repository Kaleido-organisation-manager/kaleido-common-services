using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Handlers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLifeCycleHandler<TEntity, TRevision, TBuilder>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    where TBuilder : BaseRevisionBuilder<TRevision>, new()
    {
        services.AddScoped<IEntityLifecycleHandler<TEntity, TRevision>, EntityLifeCycleHandler<TEntity, TRevision, TBuilder>>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity, TRevision>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddLifeCycleHandler<TEntity, TRevision, BaseRevisionBuilder<TRevision>>();
        services.AddScoped<IEntityLifecycleHandler<TEntity, TRevision>, EntityLifeCycleHandler<TEntity, TRevision>>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    {
        services.AddLifeCycleHandler<TEntity, BaseRevisionEntity>();
        services.AddScoped<IEntityLifecycleHandler<TEntity>, EntityLifeCycleHandler<TEntity>>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler(this IServiceCollection services)
    {
        services.AddLifeCycleHandler<BaseEntity>();
        services.AddScoped<IEntityLifecycleHandler, EntityLifeCycleHandler>();

        return services;
    }
}