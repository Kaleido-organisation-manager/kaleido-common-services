using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseEntityRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public EntityContext DbContext { get; private set; }
    public IBaseEntityRepository<BaseEntity> Repository { get; private set; }

    public BaseEntityRepositoryFixture()
    {
        var services = new ServiceCollection();
        services.AddKaleidoInMemoryEntityDbContext<BaseEntity, EntityContext>("TestEntities");
        services.AddEntityRepository<BaseEntity, EntityContext>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        DbContext = _provider.GetRequiredService<EntityContext>();
        Repository = _provider.GetRequiredService<IBaseEntityRepository<BaseEntity>>();

        DbContext.Database.EnsureCreated();

    }

    public void Dispose()
    {
        _provider.Dispose();
        DbContext.Dispose();
    }

    public void ResetDatabase()
    {
        if (DbContext.Items.Any())
        {
            DbContext.Items.RemoveRange(DbContext.Items);
            DbContext.SaveChanges();
        }
    }
}