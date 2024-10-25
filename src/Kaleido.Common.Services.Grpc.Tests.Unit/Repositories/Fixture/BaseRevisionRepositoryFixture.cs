using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseRevisionRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public KaleidoDbContext<BaseRevisionEntity> DbContext { get; private set; }
    public IBaseRevisionRepository Repository { get; private set; }

    public BaseRevisionRepositoryFixture()
    {
        var services = new ServiceCollection();
        services.AddKaleidoInMemoryRevisionDbContext<BaseRevisionEntity>("TestRevisions"); ;
        services.AddScoped<IBaseRevisionRepository, BaseRevisionRepository>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        // Resolve the DbContext and Repository
        DbContext = _provider.GetRequiredService<KaleidoDbContext<BaseRevisionEntity>>();
        Repository = _provider.GetRequiredService<IBaseRevisionRepository>();

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
