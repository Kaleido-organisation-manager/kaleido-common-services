using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseRevisionRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public BaseDbContext DbContext { get; private set; }
    public IBaseRevisionRepository Repository { get; private set; }

    public BaseRevisionRepositoryFixture()
    {
        var services = new ServiceCollection();
        // Register BaseDbContext without generic parameters
        services.AddDbContext<BaseDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "TestRevisions"));

        services.AddScoped<IBaseRevisionRepository, BaseRevisionRepository>();
        services.AddScoped(s => s.GetRequiredService<BaseDbContext>().Revisions);
        services.AddScoped<DbContext>(s => s.GetRequiredService<BaseDbContext>());
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        // Resolve the DbContext and Repository
        DbContext = _provider.GetRequiredService<BaseDbContext>();
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
        if (DbContext.Revisions.Any())
        {
            DbContext.Revisions.RemoveRange(DbContext.Revisions);
            DbContext.SaveChanges();
        }
    }
}
