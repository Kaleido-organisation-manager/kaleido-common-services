using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Handlers.Fixtures;

public class EntityLifeCycleHandlerFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public EntityDbContext DbContext { get; private set; }
    public IEntityLifecycleHandler Handler { get; private set; }

    public EntityLifeCycleHandlerFixture()
    {
        var services = new ServiceCollection();
        services.AddDbContext<EntityDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "LifeCycleTests"));
        services.AddScoped<IBaseEntityRepository, BaseEntityRepository>();
        services.AddScoped<IBaseRevisionRepository, BaseRevisionRepository>();
        services.AddScoped<IEntityLifecycleHandler, EntityLifeCycleHandler>();
        services.AddScoped(s => s.GetRequiredService<EntityDbContext>().Entities);
        services.AddScoped(s => s.GetRequiredService<EntityDbContext>().Revisions);
        services.AddScoped<DbContext>(s => s.GetRequiredService<EntityDbContext>());
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        DbContext = _provider.GetRequiredService<EntityDbContext>();
        Handler = _provider.GetRequiredService<IEntityLifecycleHandler>();

        DbContext.Database.EnsureCreated();

    }

    public void Dispose()
    {
        _provider.Dispose();
        DbContext.Dispose();
    }

    public void ResetDatabase()
    {
        if (DbContext.Entities.Any())
        {
            DbContext.Entities.RemoveRange(DbContext.Entities);
            DbContext.SaveChanges();
        }

        if (DbContext.Revisions.Any())
        {
            DbContext.Revisions.RemoveRange(DbContext.Revisions);
            DbContext.SaveChanges();
        }
    }
}