using Kaleido.Common.Services.Grpc.Configuration;
using Kaleido.Common.Services.Grpc.Configuration.Extensions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseEntityRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public KaleidoDbContext<BaseEntity> DbContext { get; private set; }
    public IBaseEntityRepository Repository { get; private set; }

    public BaseEntityRepositoryFixture()
    {
        var services = new ServiceCollection();
        services.AddKaleidoInMemoryEntityDbContext<BaseEntity>("TestEntities");
        services.AddScoped<IBaseEntityRepository, BaseEntityRepository>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        DbContext = _provider.GetRequiredService<KaleidoDbContext<BaseEntity>>();
        Repository = _provider.GetRequiredService<IBaseEntityRepository>();

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