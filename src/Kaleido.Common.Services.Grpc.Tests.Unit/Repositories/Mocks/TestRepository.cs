using Kaleido.Common.Services.Grpc.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;

public class TestRepository : BaseRepository<TestEntity, TestDbContext>, ITestRepository
{
    public TestRepository(ILogger<object> logger, TestDbContext dbContext)
        : base(logger, dbContext, dbContext.TestEntities)
    {
    }
}