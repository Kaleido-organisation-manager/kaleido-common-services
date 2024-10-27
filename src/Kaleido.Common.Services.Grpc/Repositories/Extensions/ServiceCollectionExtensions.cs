using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Repositories.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddEntityRepository<TEntity>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    {
        services.AddScoped<IBaseEntityRepository<TEntity>, BaseEntityRepository<TEntity>>();
        return services;
    }

    public static IServiceCollection AddEntityRepository(this IServiceCollection services)
    {
        services.AddScoped<IBaseEntityRepository<BaseEntity>, BaseEntityRepository<BaseEntity>>();
        services.AddScoped<IBaseEntityRepository, BaseEntityRepository>();
        return services;
    }

    public static IServiceCollection AddRevisionRepository<TRevision, TBuilder>(this IServiceCollection services)
    where TRevision : BaseRevisionEntity, new()
    where TBuilder : BaseRevisionBuilder<TRevision>, new()
    {
        services.AddScoped<IBaseRevisionRepository<TRevision, TBuilder>, BaseRevisionRepository<TRevision, TBuilder>>();
        return services;
    }

    public static IServiceCollection AddRevisionRepository<TRevision>(this IServiceCollection services)
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddRevisionRepository<TRevision, BaseRevisionBuilder<TRevision>>();
        services.AddScoped<IBaseRevisionRepository<TRevision>, BaseRevisionRepository<TRevision>>();
        return services;
    }

    public static IServiceCollection AddRevisionRepository(this IServiceCollection services)
    {
        services.AddRevisionRepository<BaseRevisionEntity>();
        services.AddScoped<IBaseRevisionRepository, BaseRevisionRepository>();
        return services;
    }
}