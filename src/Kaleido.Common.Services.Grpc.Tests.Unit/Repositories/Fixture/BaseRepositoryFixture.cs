using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq.AutoMock;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

public class BaseRepositoryFixture : IDisposable
{
    private ServiceProvider _provider { get; set; }

    public TestDbContext DbContext { get; private set; }
    public ITestRepository Repository { get; private set; }

    public BaseRepositoryFixture()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: "TestEntities"));
        services.AddScoped<ITestRepository, TestRepository>();
        services.AddLogging();

        _provider = services.BuildServiceProvider();

        DbContext = _provider.GetRequiredService<TestDbContext>();
        Repository = _provider.GetRequiredService<ITestRepository>();

        DbContext.Database.EnsureCreated();

    }

    public void Dispose()
    {
        _provider.Dispose();
        DbContext.Dispose();
    }

    public void ResetDatabase()
    {
        DbContext.TestEntities.RemoveRange(DbContext.TestEntities);
        DbContext.SaveChanges();
    }
}