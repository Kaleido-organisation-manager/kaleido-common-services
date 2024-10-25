using Kaleido.Common.Services.Grpc.Repositories;
using Kaleido.Common.Services.Grpc.Repositories.Interfaces;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseEntityRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public BaseDbContext DbContext { get; private set; }
    public IBaseEntityRepository Repository { get; private set; }

    public BaseEntityRepositoryFixture()
    {
        var services = new ServiceCollection();
        services.AddDbContext<BaseDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "TestEntities"));
        services.AddScoped<IBaseEntityRepository, BaseEntityRepository>();
        services.AddScoped(s => s.GetRequiredService<BaseDbContext>().Entities);
        services.AddScoped<DbContext>(s => s.GetRequiredService<BaseDbContext>());
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        DbContext = _provider.GetRequiredService<BaseDbContext>();
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
        if (DbContext.Entities.Any())
        {
            DbContext.Entities.RemoveRange(DbContext.Entities);
            DbContext.SaveChanges();
        }
    }
}