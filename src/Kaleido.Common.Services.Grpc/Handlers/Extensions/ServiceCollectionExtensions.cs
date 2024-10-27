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
        services.AddRevisionRepository<TRevision, TBuilder>();
        services.AddEntityRepository<TEntity>();
        services.AddScoped<IEntityLifecycleHandler<TEntity, TRevision>, EntityLifeCycleHandler<TEntity, TRevision, TBuilder>>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity, TRevision, TBuilder>(this IServiceCollection services, string connectionString, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingRevisionMethods = null)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    where TBuilder : BaseRevisionBuilder<TRevision>, new()
    {
        services.AddKaleidoDbContext(connectionString, onCreatingEntityMethods: onCreatingEntityMethods, onCreatingRevisionMethods: onCreatingRevisionMethods);
        services.AddLifeCycleHandler<TEntity, TRevision, TBuilder>();
        return services;
    }

    public static IServiceCollection AddInMemoryLifeCycleHandler<TEntity, TRevision, TBuilder>(this IServiceCollection services, string databaseName, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingRevisionMethods = null)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    where TBuilder : BaseRevisionBuilder<TRevision>, new()
    {
        services.AddKaleidoInMemoryDbContext(databaseName, onCreatingEntityMethods: onCreatingEntityMethods, onCreatingRevisionMethods: onCreatingRevisionMethods);
        services.AddLifeCycleHandler<TEntity, TRevision, TBuilder>();
        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity, TRevision>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddRevisionRepository<TRevision>();
        services.AddEntityRepository<TEntity>();
        services.AddScoped<IEntityLifecycleHandler<TEntity, TRevision>, EntityLifeCycleHandler<TEntity, TRevision>>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity, TRevision>(this IServiceCollection services, string connectionString, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingRevisionMethods = null)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddKaleidoDbContext(connectionString, onCreatingEntityMethods: onCreatingEntityMethods, onCreatingRevisionMethods: onCreatingRevisionMethods);
        services.AddLifeCycleHandler<TEntity, TRevision>();
        return services;
    }

    public static IServiceCollection AddInMemoryLifeCycleHandler<TEntity, TRevision>(this IServiceCollection services, string databaseName, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingRevisionMethods = null)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddKaleidoInMemoryDbContext(databaseName, onCreatingEntityMethods: onCreatingEntityMethods, onCreatingRevisionMethods: onCreatingRevisionMethods);
        services.AddLifeCycleHandler<TEntity, TRevision>();
        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity>(this IServiceCollection services)
    where TEntity : BaseEntity, new()
    {
        services.AddRevisionRepository();
        services.AddEntityRepository<TEntity>();
        services.AddScoped<IEntityLifecycleHandler<TEntity>, EntityLifeCycleHandler<TEntity>>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler<TEntity>(this IServiceCollection services, string connectionString, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null)
    where TEntity : BaseEntity, new()
    {
        services.AddKaleidoDbContext<TEntity, BaseRevisionEntity>(connectionString, onCreatingEntityMethods: onCreatingEntityMethods);
        services.AddLifeCycleHandler<TEntity>();
        return services;
    }

    public static IServiceCollection AddInMemoryLifeCycleHandler<TEntity>(this IServiceCollection services, string databaseName, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null)
    where TEntity : BaseEntity, new()
    {
        services.AddKaleidoInMemoryDbContext<TEntity, BaseRevisionEntity>(databaseName, onCreatingEntityMethods: onCreatingEntityMethods);
        services.AddLifeCycleHandler<TEntity>();
        return services;
    }

    public static IServiceCollection AddLifeCycleHandler(this IServiceCollection services)
    {
        services.AddRevisionRepository();
        services.AddEntityRepository();
        services.AddScoped<IEntityLifecycleHandler, EntityLifeCycleHandler>();

        return services;
    }

    public static IServiceCollection AddLifeCycleHandler(this IServiceCollection services, string connectionString)
    {
        services.AddKaleidoDbContext<BaseEntity, BaseRevisionEntity>(connectionString);
        services.AddLifeCycleHandler();
        return services;
    }

    public static IServiceCollection AddInMemoryLifeCycleHandler(this IServiceCollection services, string databaseName)
    {
        services.AddKaleidoInMemoryDbContext<BaseEntity, BaseRevisionEntity>(databaseName);
        services.AddLifeCycleHandler();
        return services;
    }
}