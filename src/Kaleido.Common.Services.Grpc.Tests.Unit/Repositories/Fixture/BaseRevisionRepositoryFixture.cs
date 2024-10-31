using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories.Extensions;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseRevisionRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public EntityRevisionContext DbContext { get; private set; }
    public IBaseRevisionRepository<BaseRevisionEntity> Repository { get; private set; }

    public BaseRevisionRepositoryFixture()
    {
        var services = new ServiceCollection();
        services.AddKaleidoInMemoryRevisionDbContext<BaseRevisionEntity, EntityRevisionContext>("TestRevisions"); ;
        services.AddRevisionRepository<BaseRevisionEntity, EntityRevisionContext>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        // Resolve the DbContext and Repository
        DbContext = _provider.GetRequiredService<EntityRevisionContext>();
        Repository = _provider.GetRequiredService<IBaseRevisionRepository<BaseRevisionEntity>>();

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
