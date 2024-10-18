using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Builders;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Mocks;
using Microsoft.EntityFrameworkCore;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories;

public class BaseRepositoryTests : IClassFixture<BaseRepositoryFixture>
{
    private readonly BaseRepositoryFixture _fixture;

    public BaseRepositoryTests(BaseRepositoryFixture fixture)
    {
        _fixture = fixture;
        _fixture.ResetDatabase();
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveEntity()
    {

        var entity = new TestEntityBuilder().Build();
        _fixture.DbContext.TestEntities.Add(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.Repository.GetActiveAsync(entity.Key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Key, result.Key);
        Assert.Equal(EntityStatus.Active, result.Status);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsNull_WhenEntityIsInactive()
    {
        // Arrange
        var entity = new TestEntityBuilder().WithStatus(EntityStatus.Archived).Build();
        _fixture.DbContext.TestEntities.Add(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.Repository.GetActiveAsync(entity.Key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _fixture.Repository.GetActiveAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsNull_WhenEntityIsDeleted()
    {
        // Arrange
        var entity = new TestEntityBuilder().WithStatus(EntityStatus.Deleted).Build();
        _fixture.DbContext.TestEntities.Add(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.Repository.GetActiveAsync(entity.Key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsAllActiveEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntityBuilder().Build(),
            new TestEntityBuilder().Build(),
            new TestEntityBuilder().WithStatus(EntityStatus.Archived).Build(),
            new TestEntityBuilder().WithStatus(EntityStatus.Deleted).Build(),
        };
        _fixture.DbContext.TestEntities.AddRange(entities);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.Repository.GetAllActiveAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.Equal(EntityStatus.Active, e.Status));
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsEmptyList_WhenNoActiveEntitiesExist()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntityBuilder().WithStatus(EntityStatus.Archived).Build(),
            new TestEntityBuilder().WithStatus(EntityStatus.Deleted).Build(),
        };
        _fixture.DbContext.TestEntities.AddRange(entities);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.Repository.GetAllActiveAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntityBuilder().WithKey(Guid.NewGuid()).Build(),
            new TestEntityBuilder().WithKey(Guid.NewGuid()).Build(),
            new TestEntityBuilder().WithKey(Guid.NewGuid()).Build(),
        };
        _fixture.DbContext.TestEntities.AddRange(entities);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var result = await _fixture.Repository.GetAllAsync();

        // Assert
        Assert.Equal(entities.Count, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoEntitiesExist()
    {
        // Act
        var result = await _fixture.Repository.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByStatusAsync_ReturnsEntitiesWithGivenStatus()
    {
        // Arrange
        var activeEntities = new List<TestEntity>
        {
            new TestEntityBuilder().Build(),
            new TestEntityBuilder().Build(),
        };
        var archivedEntities = new List<TestEntity>
        {
            new TestEntityBuilder().WithStatus(EntityStatus.Archived).Build(),
            new TestEntityBuilder().WithStatus(EntityStatus.Archived).Build(),
        };
        var deletedEntities = new List<TestEntity>
        {
            new TestEntityBuilder().WithStatus(EntityStatus.Deleted).Build(),
            new TestEntityBuilder().WithStatus(EntityStatus.Deleted).Build(),
        };
        _fixture.DbContext.TestEntities.AddRange(activeEntities.Concat(archivedEntities).Concat(deletedEntities));
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var activeResult = await _fixture.Repository.GetAllByStatusAsync(EntityStatus.Active);
        var archivedResult = await _fixture.Repository.GetAllByStatusAsync(EntityStatus.Archived);
        var deletedResult = await _fixture.Repository.GetAllByStatusAsync(EntityStatus.Deleted);

        // Assert
        Assert.Equal(activeEntities.Count, activeResult.Count());
        Assert.Equal(archivedEntities.Count, archivedResult.Count());
        Assert.Equal(deletedEntities.Count, deletedResult.Count());
    }

    [Fact]
    public async Task GetAllByStatusAsync_ReturnsEmptyList_WhenNoEntitiesWithGivenStatusExist()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntityBuilder().WithStatus(EntityStatus.Archived).Build(),
            new TestEntityBuilder().WithStatus(EntityStatus.Deleted).Build(),
        };
        _fixture.DbContext.TestEntities.AddRange(entities);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var activeResult = await _fixture.Repository.GetAllByStatusAsync(EntityStatus.Active);

        // Assert
        Assert.Empty(activeResult);
    }

    [Fact]
    public async Task GetAllByStatusAsync_ReturnsEntitiesWithGivenStatus_WhenMultipleEntitiesWithSameStatusExist()
    {
        // Arrange
        var activeEntities = new List<TestEntity>
        {
            new TestEntityBuilder().Build(),
            new TestEntityBuilder().Build(),
        };
        _fixture.DbContext.TestEntities.AddRange(activeEntities);
        await _fixture.DbContext.SaveChangesAsync();

        // Act 
        var result = await _fixture.Repository.GetAllByStatusAsync(EntityStatus.Active);

        // Assert
        Assert.Equal(activeEntities.Count, result.Count());
    }

    [Fact]
    public async Task CreateAsync_CreatesEntity()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();

        // Act
        var result = await _fixture.Repository.CreateAsync(entity);
        var createdEntity = await _fixture.DbContext.TestEntities.FindAsync(result.Id);

        // Assert
        Assert.NotNull(createdEntity);
        Assert.Equal(entity.Key, createdEntity.Key);
        Assert.Equal(EntityStatus.Active, createdEntity.Status);
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenEntityExists()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.Repository.CreateAsync(entity);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _fixture.Repository.CreateAsync(entity));
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityKeyIsEmpty()
    {
        // Arrange
        var entity = new TestEntityBuilder().WithKey(Guid.Empty).WithStatus(EntityStatus.Active).Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _fixture.Repository.CreateAsync(entity));
    }

    [Fact]
    public async Task CreateAsync_ThrowsArgumentException_WhenEntityStatusIsNotActive()
    {
        // Arrange
        var entity = new TestEntityBuilder().WithKey(Guid.NewGuid()).WithStatus(EntityStatus.Archived).Build();

        // Act & Assert 
        await Assert.ThrowsAsync<ArgumentException>(async () => await _fixture.Repository.CreateAsync(entity));
    }


    [Fact]
    public async Task UpdateAsync_UpdatesEntity()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.DbContext.TestEntities.AddAsync(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var updatedEntity = await _fixture.Repository.UpdateAsync(entity);
        var storedUpdatedEntity = await _fixture.DbContext.TestEntities.FindAsync(updatedEntity.Id);
        var keyEntities = await _fixture.DbContext.TestEntities.Where(e => e.Key == entity.Key).ToListAsync();

        // Assert
        Assert.NotNull(updatedEntity);
        Assert.NotNull(storedUpdatedEntity);
        Assert.Equal(entity.Key, updatedEntity.Key);
        Assert.Equal(EntityStatus.Active, updatedEntity.Status);
        Assert.Equal(2, updatedEntity.Revision);

        Assert.Equal(2, keyEntities.Count);
        Assert.All(keyEntities, e => Assert.Equal(entity.Key, e.Key));
        Assert.Equal(EntityStatus.Active, keyEntities.Where(e => e.Revision == 2).FirstOrDefault()?.Status);
        Assert.Equal(EntityStatus.Archived, keyEntities.Where(e => e.Revision == 1).FirstOrDefault()?.Status);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityKeyIsEmpty()
    {
        // Arrange
        var entity = new TestEntityBuilder().WithKey(Guid.Empty).Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _fixture.Repository.UpdateAsync(entity));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsArgumentException_WhenEntityDoesNotExist()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _fixture.Repository.UpdateAsync(entity));
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatus()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.DbContext.TestEntities.AddAsync(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var updatedEntity = await _fixture.Repository.UpdateStatusAsync(entity.Key, EntityStatus.Archived);
        var storedUpdatedEntity = await _fixture.DbContext.TestEntities.FindAsync(updatedEntity?.Id);

        // Assert
        Assert.NotNull(updatedEntity);
        Assert.NotNull(storedUpdatedEntity);
        Assert.Equal(EntityStatus.Archived, updatedEntity.Status);
        Assert.Equal(EntityStatus.Archived, storedUpdatedEntity.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _fixture.Repository.UpdateStatusAsync(Guid.NewGuid(), EntityStatus.Archived);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_DeletesEntity()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.DbContext.TestEntities.AddAsync(entity);
        await _fixture.DbContext.SaveChangesAsync();

        //   Act
        var deletedEntity = await _fixture.Repository.DeleteAsync(entity.Key);
        var storedDeletedEntity = await _fixture.DbContext.TestEntities.FindAsync(deletedEntity?.Id);

        // Assert
        Assert.NotNull(deletedEntity);
        Assert.NotNull(storedDeletedEntity);
        Assert.Equal(EntityStatus.Deleted, deletedEntity.Status);
        Assert.Equal(EntityStatus.Deleted, storedDeletedEntity.Status);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _fixture.Repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ReturnsRevisions()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.DbContext.TestEntities.AddAsync(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var revisions = await _fixture.Repository.GetAllRevisionsAsync(entity.Key);

        // Assert
        Assert.Single(revisions);
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ReturnsAllRevisions()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.Repository.CreateAsync(entity);
        await _fixture.Repository.UpdateAsync(entity);
        await _fixture.Repository.UpdateAsync(entity);

        // Act
        var revisions = await _fixture.Repository.GetAllRevisionsAsync(entity.Key);

        // Assert
        Assert.Equal(3, revisions.Count());
    }

    [Fact]
    public async Task GetAllRevisionsAsync_ReturnsEmptyList_WhenNoRevisionsExist()
    {
        // Act
        var revisions = await _fixture.Repository.GetAllRevisionsAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(revisions);
    }

    [Fact]
    public async Task GetRevisionAsync_ReturnsRevision()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.Repository.CreateAsync(entity);
        await _fixture.Repository.UpdateAsync(entity);

        // Act
        var revision = await _fixture.Repository.GetRevisionAsync(entity.Key, 2);

        // Assert
        Assert.NotNull(revision);
        Assert.Equal(entity.Key, revision.Key);
        Assert.Equal(EntityStatus.Active, revision.Status);
        Assert.Equal(2, revision.Revision);
    }

    [Fact]
    public async Task GetRevisionAsync_ReturnsNull_WhenRevisionDoesNotExist()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.DbContext.TestEntities.AddAsync(entity);
        await _fixture.DbContext.SaveChangesAsync();

        // Act
        var revision = await _fixture.Repository.GetRevisionAsync(entity.Key, 2);

        // Assert
        Assert.Null(revision);
    }

    [Fact]
    public async Task GetRevisionAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Act
        var revision = await _fixture.Repository.GetRevisionAsync(Guid.NewGuid(), 1);

        // Assert
        Assert.Null(revision);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenEntityExists()
    {
        // Arrange
        var entity = new TestEntityBuilder().Build();
        await _fixture.Repository.CreateAsync(entity);

        // Act
        var result = await _fixture.Repository.ExistsAsync(entity.Key);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _fixture.Repository.ExistsAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

}
