using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Exceptions;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Tests.Unit.Handlers.Fixtures;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Handlers
{
    public class EntityLifeCycleHandlerTests : IClassFixture<EntityLifeCycleHandlerFixture>
    {
        private readonly EntityLifeCycleHandlerFixture _fixture;

        public EntityLifeCycleHandlerTests(EntityLifeCycleHandlerFixture fixture)
        {
            _fixture = fixture;
            fixture.ResetDatabase();
        }

        [Fact]
        public async Task CreateAsync_CreatesEntityAndRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };

            // Act
            var result = await _fixture.Handler.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Entity);
            Assert.NotNull(result.Revision);
            Assert.Equal(entity.Id, result.Entity.Id);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Handler.CreateAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_DeletesEntityAndReturnsResult()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.DeleteAsync(createResult.Key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
            Assert.NotNull(result.Revision);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentNullException_WhenEntityDoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<RevisionNotFoundException>(async () =>
                await _fixture.Handler.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task RestoreAsync_RestoresEntityAndReturnsResult()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            var deleteResult = await _fixture.Handler.DeleteAsync(createResult.Key);

            // Act
            var restoreResult = await _fixture.Handler.RestoreAsync(deleteResult.Key);

            // Assert
            Assert.NotNull(restoreResult);
            Assert.NotNull(restoreResult.Entity);
            Assert.NotNull(restoreResult.Revision);
            Assert.Equal(createResult.Key, restoreResult.Key);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEntityAndReturnsResult()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            var updatedEntity = new BaseEntity { Id = Guid.NewGuid() };

            // Act
            var result = await _fixture.Handler.UpdateAsync(createResult.Key, updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedEntity.Id, result.Entity.Id);
            Assert.NotNull(result.Revision);
            Assert.Equal(2, result.Revision.Revision);
            Assert.Equal(createResult.Key, result.Key);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsRevisionNotFoundException_WhenEntityDoesNotExist()
        {
            // Arrange
            var updatedEntity = new BaseEntity() { Id = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<RevisionNotFoundException>(async () =>
                await _fixture.Handler.UpdateAsync(Guid.NewGuid(), updatedEntity));
        }

        [Fact]
        public async Task GetAsync_ReturnsEntityAndRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.GetAsync(createResult.Key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
            Assert.NotNull(result.Revision);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenEntityDoesNotExist()
        {
            // Act
            var result = await _fixture.Handler.GetAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_ReturnsEntity_WhenEntityIsDeleted()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            await _fixture.Handler.DeleteAsync(createResult.Key);

            // Act
            var result = await _fixture.Handler.GetAsync(createResult.Key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
            Assert.NotNull(result.Revision);
        }

        [Fact]
        public async Task FindAsync_ReturnsMatchingEntity()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.FindAsync(e => e.Id == createResult.Entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.All(lc => lc.Entity.Id == entity.Id));
        }

        [Fact]
        public async Task FindAsync_ReturnsEmpty_WhenEntityNotFound()
        {
            // Act
            var result = await _fixture.Handler.FindAsync(e => e.Id == Guid.NewGuid());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            // Arrange
            var entity1 = new BaseEntity() { Id = Guid.NewGuid() };
            var entity2 = new BaseEntity() { Id = Guid.NewGuid() };
            await _fixture.Handler.CreateAsync(entity1);
            await _fixture.Handler.CreateAsync(entity2);

            // Act
            var result = await _fixture.Handler.GetAllAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task FindAllAsync_ReturnsMatchingEntities_WithRevisionFilter()
        {
            // Arrange
            var entity1 = new BaseEntity { Id = Guid.NewGuid() };
            var entity2 = new BaseEntity { Id = Guid.NewGuid() };
            await _fixture.Handler.CreateAsync(entity1);
            await _fixture.Handler.CreateAsync(entity2);

            // Act
            var result = await _fixture.Handler.FindAllAsync(e => e.Id == entity1.Id, r => r.Action == RevisionAction.Created);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(entity1.Id, result.First().Entity.Id);
        }

        [Fact]
        public async Task FindAllAsync_ReturnsEmpty_WithRevisionFilterWithNoMatch()
        {
            // Arrange
            var entity1 = new BaseEntity { Id = Guid.NewGuid() };
            var entity2 = new BaseEntity { Id = Guid.NewGuid() };
            await _fixture.Handler.CreateAsync(entity1);
            await _fixture.Handler.CreateAsync(entity2);

            // Act
            var result = await _fixture.Handler.FindAllAsync(e => e.Id == entity1.Id, r => r.Action == RevisionAction.Deleted);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task FindAsync_ReturnsEntity_WithRevisionFilter()
        {
            // Arrange
            var entity = new BaseEntity { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.FindAsync(e => e.Id == entity.Id, r => r.Action == RevisionAction.Created);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(entity.Id, result.First().Entity.Id);
        }

        [Fact]
        public async Task FindAsync_ReturnsEmpty_WithRevisionFilterWithNoMatch()
        {
            // Arrange
            var entity = new BaseEntity { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.FindAsync(e => e.Id == entity.Id, r => r.Action == RevisionAction.Deleted);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsEntity_AtPointInTime()
        {
            // Arrange
            var entity = new BaseEntity { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            var updateResult = await _fixture.Handler.UpdateAsync(createResult.Key, new BaseEntity { Id = Guid.NewGuid() });

            // Act
            var pointInTime = createResult.Revision.CreatedAt.AddMicroseconds((updateResult.Revision.CreatedAt - createResult.Revision.CreatedAt).TotalMicroseconds / 2);
            var result = await _fixture.Handler.GetHistoricAsync(createResult.Key, pointInTime);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsNull_WhenEntityDoesNotExist()
        {
            // Act
            var result = await _fixture.Handler.GetHistoricAsync(Guid.NewGuid(), DateTime.UtcNow);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsNull_WhenNoRevisionExistsAtPointInTime()
        {
            // Arrange
            var entity = new BaseEntity { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.GetHistoricAsync(createResult.Key, createResult.Revision.CreatedAt.AddSeconds(-1));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsLatestRevision_WhenPointInTimeIsAfterAllRevisions()
        {
            // Arrange
            var entity = new BaseEntity { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.GetHistoricAsync(createResult.Key, createResult.Revision.CreatedAt.AddSeconds(1));

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createResult.Revision.Id, result.Revision.Id);
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsNull_WhenPointInTimeIsBeforeAllRevisions()
        {
            // Arrange
            var entity = new BaseEntity { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.GetHistoricAsync(createResult.Key, createResult.Revision.CreatedAt.AddSeconds(-1));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsRevisionNotFoundException_WhenEntityIsNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<RevisionNotFoundException>(async () => await _fixture.Handler.UpdateAsync(Guid.NewGuid(), new BaseEntity { Id = Guid.NewGuid() }));
        }

        [Fact]
        public async Task DeleteAsync_ThrowsRevisionNotFoundException_WhenEntityIsNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<RevisionNotFoundException>(async () => await _fixture.Handler.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task RestoreAsync_ThrowsRevisionNotFoundException_WhenEntityIsNotFound()
        {
            // Act & Assert
            await Assert.ThrowsAsync<RevisionNotFoundException>(async () => await _fixture.Handler.RestoreAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenEntityIsNotFound()
        {
            // Act
            var result = await _fixture.Handler.GetAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsNull_WhenEntityIsNotFound()
        {
            // Act
            var result = await _fixture.Handler.GetHistoricAsync(Guid.NewGuid(), DateTime.UtcNow);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetHistoricAsync_ThrowsArgumentNullException_WhenKeyIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _fixture.Handler.GetHistoricAsync(Guid.Empty, DateTime.UtcNow));
        }

        [Fact]
        public async Task CreateAsync_WithPrefilledRevision_CreatesEntityAndRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var revision = new BaseRevisionEntity() { Key = Guid.NewGuid() };

            // Act
            var result = await _fixture.Handler.CreateAsync(entity, revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
            Assert.Equal(revision.Key, result.Revision.Key);
        }

        [Fact]
        public async Task CreateAsync_WithNullRevision_CreatesEntityAndRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };

            // Act
            var result = await _fixture.Handler.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
            Assert.NotNull(result.Revision);
        }

        [Fact]
        public async Task CreateAsync_WithNullEntity_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _fixture.Handler.CreateAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_WithCustomRevision_UsesProvidedRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var revision = new BaseRevisionEntity() { Key = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.DeleteAsync(createResult.Key, revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Entity.Id);
            Assert.Equal(revision.Key, result.Revision.Key);
        }

        [Fact]
        public async Task UpdateAsync_WithCustomRevision_UsesProvidedRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var revision = new BaseRevisionEntity() { Key = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);

            // Act
            var result = await _fixture.Handler.UpdateAsync(createResult.Key, new BaseEntity { Id = Guid.NewGuid() }, revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revision.Key, result.Revision.Key);
        }

        [Fact]
        public async Task RestoreAsync_WithCustomRevision_UsesProvidedRevision()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var revision = new BaseRevisionEntity() { Key = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            await _fixture.Handler.DeleteAsync(createResult.Key);

            // Act
            var result = await _fixture.Handler.RestoreAsync(createResult.Key, revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revision.Key, result.Revision.Key);
        }

        [Fact]
        public async Task FindAllAsync_WhenEntityIsDeleted_ReturnsRevisions()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            await _fixture.Handler.DeleteAsync(createResult.Key);

            // Act
            var result = await _fixture.Handler.FindAllAsync(e => e.Id == entity.Id);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task FindAllAsync_WhenEntityIsDeletedAndFilteredByRevision_ReturnsRevisions()
        {
            // Arrange
            var entity = new BaseEntity() { Id = Guid.NewGuid() };
            var createResult = await _fixture.Handler.CreateAsync(entity);
            await _fixture.Handler.DeleteAsync(createResult.Key);

            // Act
            var result = await _fixture.Handler.FindAllAsync(e => e.Id == entity.Id, r => r.Action == RevisionAction.Deleted);

            // Assert
            Assert.NotEmpty(result);
            Assert.Single(result);
        }
    }
}

