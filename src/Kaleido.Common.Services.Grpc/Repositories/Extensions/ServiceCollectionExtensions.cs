using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Repositories.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddEntityRepository<TEntity, EntityContext>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    where EntityContext : DbContext, IKaleidoDbContext<TEntity>
    {
        services.AddScoped<IBaseEntityRepository<TEntity>, BaseEntityRepository<TEntity, EntityContext>>();
        return services;
    }

    public static IServiceCollection AddEntityRepository<EntityContext>(this IServiceCollection services)
    where EntityContext : DbContext, IKaleidoDbContext<BaseEntity>
    {
        services.AddEntityRepository<BaseEntity, EntityContext>();
        // services.AddScoped<IBaseEntityRepository<BaseEntity>, BaseEntityRepository<BaseEntity>>();
        services.AddScoped<IBaseEntityRepository, BaseEntityRepository<EntityContext>>();
        return services;
    }

    public static IServiceCollection AddRevisionRepository<TRevision, TBuilder, RevisionContext>(this IServiceCollection services)
    where TRevision : BaseRevisionEntity, new()
    where TBuilder : BaseRevisionBuilder<TRevision>, new()
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {
        services.AddScoped<IBaseRevisionRepository<TRevision, TBuilder>, BaseRevisionRepository<TRevision, TBuilder, RevisionContext>>();
        return services;
    }

    public static IServiceCollection AddRevisionRepository<TRevision, RevisionContext>(this IServiceCollection services)
    where TRevision : BaseRevisionEntity, new()
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {
        services.AddRevisionRepository<TRevision, BaseRevisionBuilder<TRevision>, RevisionContext>();
        services.AddScoped<IBaseRevisionRepository<TRevision>, BaseRevisionRepository<TRevision, RevisionContext>>();
        return services;
    }

    public static IServiceCollection AddRevisionRepository<RevisionContext>(this IServiceCollection services)
    where RevisionContext : DbContext, IKaleidoDbContext<BaseRevisionEntity>
    {
        services.AddRevisionRepository<BaseRevisionEntity, RevisionContext>();
        services.AddScoped<IBaseRevisionRepository, BaseRevisionRepository<RevisionContext>>();
        return services;
    }
}