using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Handlers;
using Kaleido.Common.Services.Grpc.Handlers.Extensions;
using Kaleido.Common.Services.Grpc.Handlers.Interfaces;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Common.Services.Grpc.Tests.Unit.Handlers.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Handlers.Fixtures;

public class EntityLifeCycleHandlerFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public EntityContext EntityDbContext { get; private set; }
    public EntityRevisionContext RevisionDbContext { get; private set; }
    public IEntityLifecycleHandler<BaseEntity, BaseRevisionEntity> Handler { get; private set; }

    public EntityLifeCycleHandlerFixture()
    {
        var services = new ServiceCollection();
        services.AddKaleidoInMemoryEntityDbContext<BaseEntity, EntityContext>("LifeCycleTests");
        services.AddKaleidoInMemoryRevisionDbContext<BaseRevisionEntity, EntityRevisionContext>("LifeCycleTests");

        services.AddEntityRepository<BaseEntity, EntityContext>();
        services.AddRevisionRepository<BaseRevisionEntity, EntityRevisionContext>();
        services.AddLifeCycleHandler<BaseEntity, BaseRevisionEntity>();

        // services.AddInMemoryLifeCycleHandler<BaseEntity, BaseRevisionEntity, EntityContext, EntityRevisionContext>("LifeCycleTests");
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        EntityDbContext = _provider.GetRequiredService<EntityContext>();
        RevisionDbContext = _provider.GetRequiredService<EntityRevisionContext>();
        Handler = _provider.GetRequiredService<IEntityLifecycleHandler<BaseEntity, BaseRevisionEntity>>();

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