using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Builders;

public class TestEntityBuilder
{
    private TestEntity _testEntity = new()
    {
        Id = Guid.NewGuid(),
        Key = Guid.NewGuid(),
        Status = EntityStatus.Active,
        Revision = 1,
        CreatedAt = DateTime.UtcNow,
    };

    public TestEntityBuilder WithKey(Guid key)
    {
        _testEntity.Key = key;
        return this;
    }

    public TestEntityBuilder WithStatus(EntityStatus status)
    {
        _testEntity.Status = status;
        return this;
    }

    public TestEntityBuilder WithRevision(int revision)
    {
        _testEntity.Revision = revision;
        return this;
    }

    public TestEntity Build() => _testEntity;

}