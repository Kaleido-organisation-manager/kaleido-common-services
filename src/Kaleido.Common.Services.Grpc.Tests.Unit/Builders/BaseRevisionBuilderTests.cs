using Kaleido.Common.Services.Grpc.Builders;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Builders;

public class BaseRevisionBuilderTests
{
    [Fact]
    public void Build_CreatesNewInstance_WithDefaultValues()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();

        // Act
        var result = builder.Build();

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(default, result.CreatedAt);
    }

    [Fact]
    public void WithKey_SetsKey_WhenValidGuid()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();
        var expectedKey = Guid.NewGuid();

        // Act
        var result = builder.WithKey(expectedKey).Build();

        // Assert
        Assert.Equal(expectedKey, result.Key);
    }

    [Fact]
    public void WithKey_ThrowsArgumentException_WhenEmptyGuid()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => builder.WithKey(Guid.Empty));
    }

    [Fact]
    public void WithAction_SetsAction()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();
        var expectedAction = RevisionAction.Created;

        // Act
        var result = builder.WithAction(expectedAction).Build();

        // Assert
        Assert.Equal(expectedAction, result.Action);
    }

    [Fact]
    public void FromRevision_CopiesValues_WhenRevisionProvided()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();
        var existingRevision = new BaseRevisionEntity
        {
            Key = Guid.NewGuid(),
            EntityId = Guid.NewGuid(),
            Action = RevisionAction.Created,
            Revision = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid()
        };

        // Act
        var result = builder.FromRevision(existingRevision).Build();

        // Assert
        Assert.Equal(existingRevision.Key, result.Key);
        Assert.Equal(existingRevision.EntityId, result.EntityId);
        Assert.Equal(existingRevision.Action, result.Action);
        Assert.Equal(existingRevision.Revision, result.Revision);
        Assert.Equal(existingRevision.CreatedAt, result.CreatedAt);
        Assert.Equal(existingRevision.Id, result.Id);
    }

    [Fact]
    public void FromRevision_ReturnsOriginalBuilder_WhenRevisionIsNull()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();
        var previousObject = builder.Build();

        // Act
        var result = builder.FromRevision(null).Build();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(previousObject, result);
    }

    [Fact]
    public void WithEntityId_SetsEntityId()
    {
        // Arrange
        var builder = new BaseRevisionBuilder();
        var expectedEntityId = Guid.NewGuid();

        // Act
        var result = builder.WithEntityId(expectedEntityId).Build();

        // Assert
        Assert.Equal(expectedEntityId, result.EntityId);
    }
}
