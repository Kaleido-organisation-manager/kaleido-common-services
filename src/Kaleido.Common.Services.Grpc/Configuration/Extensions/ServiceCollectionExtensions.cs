using Kaleido.Common.Services.Grpc.Configuration.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Configuration.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddKaleidoDbContext<TEntity, TRevision>(this IServiceCollection services, string connectionString, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingRevisionMethods = null)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddKaleidoEntityDbContext(connectionString, onCreatingEntityMethods);
        services.AddKaleidoRevisionDbContext(connectionString, onCreatingRevisionMethods);
        return services;
    }

    public static IServiceCollection AddKaleidoEntityDbContext<TEntity>(this IServiceCollection services, string connectionString, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingModelMethods = null)
    where TEntity : BaseEntity, new()
    {
        var modelCreatingMethods = new List<Action<EntityTypeBuilder<TEntity>>> { DefaultOnModelCreatingMethod.ForBaseEntity };
        if (onCreatingModelMethods != null && onCreatingModelMethods.Any())
        {
            modelCreatingMethods.AddRange(onCreatingModelMethods);
        }

        services.AddDbContext<KaleidoDbContext<TEntity>>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped(s => s.GetRequiredService<KaleidoDbContext<TEntity>>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoMigrationEntityDbContext<TEntity>(this IServiceCollection services, string connectionString, string assemblyName, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingModelMethods = null)
    where TEntity : BaseEntity, new()
    {
        var modelCreatingMethods = new List<Action<EntityTypeBuilder<TEntity>>> { DefaultOnModelCreatingMethod.ForBaseEntity };
        if (onCreatingModelMethods != null && onCreatingModelMethods.Any())
        {
            modelCreatingMethods.AddRange(onCreatingModelMethods);
        }

        services.AddDbContext<KaleidoDbContext<TEntity>>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(assemblyName)));
        services.AddScoped(s => s.GetRequiredService<KaleidoDbContext<TEntity>>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoRevisionDbContext<TRevision>(this IServiceCollection services, string connectionString, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingModelMethods = null)
    where TRevision : BaseRevisionEntity, new()
    {
        var modelCreatingMethods = new List<Action<EntityTypeBuilder<TRevision>>> { DefaultOnModelCreatingMethod.ForBaseEntity, DefaultOnModelCreatingMethod.ForBaseRevisionEntity };
        if (onCreatingModelMethods != null && onCreatingModelMethods.Any())
        {
            modelCreatingMethods.AddRange(onCreatingModelMethods);
        }

        services.AddDbContext<KaleidoDbContext<TRevision>>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped(s => s.GetRequiredService<KaleidoDbContext<TRevision>>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoMigrationRevisionDbContext<TRevision>(this IServiceCollection services, string connectionString, string assemblyName, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingModelMethods = null)
    where TRevision : BaseRevisionEntity, new()
    {
        var modelCreatingMethods = new List<Action<EntityTypeBuilder<TRevision>>> { DefaultOnModelCreatingMethod.ForBaseEntity, DefaultOnModelCreatingMethod.ForBaseRevisionEntity };
        if (onCreatingModelMethods != null && onCreatingModelMethods.Any())
        {
            modelCreatingMethods.AddRange(onCreatingModelMethods);
        }

        services.AddDbContext<KaleidoDbContext<TRevision>>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(assemblyName)));
        services.AddScoped(s => s.GetRequiredService<KaleidoDbContext<TRevision>>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoInMemoryDbContext<TEntity, TRevision>(this IServiceCollection services, string databaseName, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingEntityMethods = null, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingRevisionMethods = null)
    where TEntity : BaseEntity, new()
    where TRevision : BaseRevisionEntity, new()
    {
        services.AddKaleidoInMemoryEntityDbContext(databaseName, onCreatingEntityMethods);
        services.AddKaleidoInMemoryRevisionDbContext(databaseName, onCreatingRevisionMethods);
        return services;
    }

    public static IServiceCollection AddKaleidoInMemoryEntityDbContext<TEntity>(this IServiceCollection services, string databaseName, IEnumerable<Action<EntityTypeBuilder<TEntity>>>? onCreatingModelMethods = null)
    where TEntity : BaseEntity, new()
    {
        var modelCreatingMethods = new List<Action<EntityTypeBuilder<TEntity>>> { DefaultOnModelCreatingMethod.ForBaseEntity };
        if (onCreatingModelMethods != null && onCreatingModelMethods.Any())
        {
            modelCreatingMethods.AddRange(onCreatingModelMethods);
        }

        services.AddDbContext<KaleidoDbContext<TEntity>>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped(s => s.GetRequiredService<KaleidoDbContext<TEntity>>().Items);

        return services;
    }

    public static IServiceCollection AddKaleidoInMemoryRevisionDbContext<TRevision>(this IServiceCollection services, string databaseName, IEnumerable<Action<EntityTypeBuilder<TRevision>>>? onCreatingModelMethods = null)
    where TRevision : BaseRevisionEntity, new()
    {
        var modelCreatingMethods = new List<Action<EntityTypeBuilder<TRevision>>> { DefaultOnModelCreatingMethod.ForBaseEntity, DefaultOnModelCreatingMethod.ForBaseRevisionEntity };
        if (onCreatingModelMethods != null && onCreatingModelMethods.Any())
        {
            modelCreatingMethods.AddRange(onCreatingModelMethods);
        }

        services.AddDbContext<KaleidoDbContext<TRevision>>(options =>
            options.UseInMemoryDatabase(databaseName));
        services.AddScoped(s => s.GetRequiredService<KaleidoDbContext<TRevision>>().Items);

        return services;
    }
}
