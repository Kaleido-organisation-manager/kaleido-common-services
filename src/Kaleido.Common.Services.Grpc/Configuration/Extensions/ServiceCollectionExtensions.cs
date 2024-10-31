using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Configuration.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Configuration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKaleidoDbContext<TEntity, TRevision, EntityContext, RevisionContext>(this IServiceCollection services, string connectionString)
    where TEntity : BaseEntity
    where TRevision : BaseRevisionEntity
    where EntityContext : DbContext, IKaleidoDbContext<TEntity>
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {
        services.AddKaleidoEntityDbContext<TEntity, EntityContext>(connectionString);
        services.AddKaleidoRevisionDbContext<TRevision, RevisionContext>(connectionString);
        return services;
    }

    public static IServiceCollection AddKaleidoEntityDbContext<TEntity, EntityContext>(this IServiceCollection services, string connectionString)
    where TEntity : BaseEntity
    where EntityContext : DbContext, IKaleidoDbContext<TEntity>
    {
        services.AddDbContext<EntityContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped(s => s.GetRequiredService<EntityContext>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoMigrationEntityDbContext<TEntity, EntityContext>(this IServiceCollection services, string connectionString, string assemblyName)
    where TEntity : BaseEntity
    where EntityContext : DbContext, IKaleidoDbContext<TEntity>
    {

        services.AddDbContext<EntityContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(assemblyName)));
        services.AddScoped(s => s.GetRequiredService<EntityContext>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoRevisionDbContext<TRevision, RevisionContext>(this IServiceCollection services, string connectionString)
    where TRevision : BaseRevisionEntity
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {

        services.AddDbContext<RevisionContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped(s => s.GetRequiredService<RevisionContext>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoMigrationRevisionDbContext<TRevision, RevisionContext>(this IServiceCollection services, string connectionString, string assemblyName)
    where TRevision : BaseRevisionEntity, new()
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {
        services.AddDbContext<RevisionContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(assemblyName)));
        services.AddScoped(s => s.GetRequiredService<RevisionContext>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoInMemoryDbContext<TEntity, TRevision, EntityContext, RevisionContext>(this IServiceCollection services, string databaseName)
    where TEntity : BaseEntity
    where TRevision : BaseRevisionEntity
    where EntityContext : DbContext, IKaleidoDbContext<TEntity>
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {
        services.AddKaleidoInMemoryEntityDbContext<TEntity, EntityContext>(databaseName);
        services.AddKaleidoInMemoryRevisionDbContext<TRevision, RevisionContext>(databaseName);
        return services;
    }

    public static IServiceCollection AddKaleidoInMemoryEntityDbContext<TEntity, EntityContext>(this IServiceCollection services, string databaseName)
    where TEntity : BaseEntity
    where EntityContext : DbContext, IKaleidoDbContext<TEntity>
    {
        services.AddDbContext<EntityContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped(s => s.GetRequiredService<EntityContext>().Items);
        return services;
    }

    public static IServiceCollection AddKaleidoInMemoryRevisionDbContext<TRevision, RevisionContext>(this IServiceCollection services, string databaseName)
    where TRevision : BaseRevisionEntity
    where RevisionContext : DbContext, IKaleidoDbContext<TRevision>
    {
        services.AddDbContext<RevisionContext>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped(s => s.GetRequiredService<RevisionContext>().Items);

        return services;
    }
}
