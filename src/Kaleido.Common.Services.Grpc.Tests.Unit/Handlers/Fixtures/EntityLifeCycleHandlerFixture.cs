using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Handlers.Fixtures;

public class EntityLifeCycleHandlerFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public KaleidoDbContext<BaseEntity> EntityDbContext { get; private set; }
    public KaleidoDbContext<BaseRevisionEntity> RevisionDbContext { get; private set; }
    public IEntityLifecycleHandler Handler { get; private set; }

    public EntityLifeCycleHandlerFixture()
    {
        var services = new ServiceCollection();
        services.AddKaleidoInMemoryDbContext<BaseEntity, BaseRevisionEntity>("LifeCycleTests");

        services.AddScoped<IBaseEntityRepository, BaseEntityRepository>();
        services.AddScoped<IBaseRevisionRepository, BaseRevisionRepository>();
        services.AddScoped<IEntityLifecycleHandler, EntityLifeCycleHandler>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        EntityDbContext = _provider.GetRequiredService<KaleidoDbContext<BaseEntity>>();
        RevisionDbContext = _provider.GetRequiredService<KaleidoDbContext<BaseRevisionEntity>>();
        Handler = _provider.GetRequiredService<IEntityLifecycleHandler>();

        EntityDbContext.Database.EnsureCreated();
        RevisionDbContext.Database.EnsureCreated();

    }

    public void Dispose()
    {
        _provider.Dispose();
        EntityDbContext.Dispose();
        RevisionDbContext.Dispose();
    }

    public void ResetDatabase()
    {
        if (EntityDbContext.Items.Any())
        {
            EntityDbContext.Items.RemoveRange(EntityDbContext.Items);
            EntityDbContext.SaveChanges();
        }

        if (RevisionDbContext.Items.Any())
        {
            RevisionDbContext.Items.RemoveRange(RevisionDbContext.Items);
            RevisionDbContext.SaveChanges();
        }
    }
}