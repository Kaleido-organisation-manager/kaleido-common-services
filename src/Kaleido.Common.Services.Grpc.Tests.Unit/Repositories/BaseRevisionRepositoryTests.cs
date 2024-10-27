using Grpc.Core;
using Kaleido.Common.Services.Grpc.Constants;
using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories
{
    public class BaseRevisionRepositoryTests : IClassFixture<BaseRevisionRepositoryFixture>
    {
        private readonly BaseRevisionRepositoryFixture _fixture;

        public BaseRevisionRepositoryTests(BaseRevisionRepositoryFixture fixture)
        {
            _fixture = fixture;
            fixture.ResetDatabase();
        }

        [Fact]
        public async Task CreateAsync_CreatesRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision = new BaseRevisionEntity { EntityId = entityId };

            // Act
            var result = await _fixture.Repository.CreateAsync(entityId, revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Revision);
            Assert.Equal(RevisionAction.Created, result.Action);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIdIsEmptyGuid()
        {
            // Arrange
            var entityId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Repository.CreateAsync(entityId, null));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var initialRevision = await _fixture.Repository.CreateAsync(entityId);
            var updatedRevision = new BaseRevisionEntity { EntityId = Guid.NewGuid() };

            // Act
            var result = await _fixture.Repository.UpdateAsync(initialRevision.Key, updatedRevision.EntityId, updatedRevision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedRevision.EntityId, result.EntityId);
            Assert.Equal(2, result.Revision);
            Assert.Equal(RevisionAction.Updated, result.Action);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenUpdatingDeletedRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var initialRevision = await _fixture.Repository.CreateAsync(entityId);
            await _fixture.Repository.DeleteAsync(initialRevision.Key, entityId);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _fixture.Repository.UpdateAsync(initialRevision.Key, entityId, new BaseRevisionEntity()));
        }

        [Fact]
        public async Task DeleteAsync_DeletesRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision = await _fixture.Repository.CreateAsync(entityId);

            // Act
            var result = await _fixture.Repository.DeleteAsync(revision.Key, entityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entityId, result.EntityId);
            Assert.Equal(2, result.Revision);
            Assert.Equal(RevisionAction.Deleted, result.Action);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentNullException_WhenKeyIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Repository.DeleteAsync(Guid.Empty, Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_ThrowsInvalidOperationException_WhenRevisionWasAlreadyDeleted()
        {
            // Arrange
            var revision = await _fixture.Repository.CreateAsync(Guid.NewGuid());
            var deletedRevision = await _fixture.Repository.DeleteAsync(revision.Key);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _fixture.Repository.DeleteAsync(revision.Key));
        }

        [Fact]
        public async Task RestoreAsync_RestoresDeletedRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision = await _fixture.Repository.CreateAsync(entityId);
            await _fixture.Repository.DeleteAsync(revision.Key, entityId);

            // Act
            var result = await _fixture.Repository.RestoreAsync(revision.Key, entityId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entityId, result.EntityId);
            Assert.Equal(3, result.Revision);
            Assert.Equal(RevisionAction.Restored, result.Action);
        }

        [Fact]
        public async Task RestoreAsync_ThrowsArgumentNullException_WhenKeyIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Repository.RestoreAsync(Guid.Empty, Guid.NewGuid()));
        }

        [Fact]
        public async Task RestoreAsync_ThrowsInvalidOperationException_WhenRevisionWasNotDeleted()
        {
            // Arrange
            var revision = await _fixture.Repository.CreateAsync(Guid.NewGuid());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _fixture.Repository.RestoreAsync(revision.Key));
        }

        [Fact]
        public async Task RestoreAsync_ThrowsArgumentNullException_WhenRevisionSetDoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Repository.RestoreAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAsync_ReturnsRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision = await _fixture.Repository.CreateAsync(entityId);

            // Act
            var result = await _fixture.Repository.GetAsync(revision.Key, revision.Revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revision.Key, result.Key);
            Assert.Equal(revision.Revision, result.Revision);
        }

        [Fact]
        public async Task GetAsync_WithRevision_GetSpecifiedRevision()
        {
            // Arrange
            var revision = await _fixture.Repository.CreateAsync(Guid.NewGuid());
            await _fixture.Repository.UpdateAsync(revision.Key, Guid.NewGuid());

            // Act
            var result = await _fixture.Repository.GetAsync(revision.Key, revision.Revision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revision.Id, result.Id);
        }

        [Fact]
        public async Task GetAsync_WithoutRevision_GetsLatestRevision()
        {
            // Arrange
            var revision = await _fixture.Repository.CreateAsync(Guid.NewGuid());
            var updatedRevision = await _fixture.Repository.UpdateAsync(revision.Key, Guid.NewGuid());

            // Act
            var result = await _fixture.Repository.GetAsync(revision.Key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedRevision.Id, result.Id);
        }

        [Fact]
        public async Task GetAsync_ThrowsArgumentNullException_WhenKeyIsEmpty()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Repository.GetAsync(Guid.Empty, 1));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRevisions()
        {
            // Arrange
            var revision = await _fixture.Repository.CreateAsync(Guid.NewGuid());
            await _fixture.Repository.UpdateAsync(revision.Key, Guid.NewGuid());

            // Act
            var result = await _fixture.Repository.GetAllAsync(revision.Key);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllAsync_WithNoKey_ReturnsLatestVersionOfEachRevision()
        {
            // Arrange
            var revision1 = await _fixture.Repository.CreateAsync(Guid.NewGuid());
            var updatedRevision1 = await _fixture.Repository.UpdateAsync(revision1.Key, Guid.NewGuid());

            var revision2 = await _fixture.Repository.CreateAsync(Guid.NewGuid());
            var updatedRevision2 = await _fixture.Repository.UpdateAsync(revision2.Key, Guid.NewGuid());
            var deletedRevision2 = await _fixture.Repository.DeleteAsync(revision2.Key);

            // Act
            var result = await _fixture.Repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
            var revision1Result = result.FirstOrDefault(r => r.Key == revision1.Key);
            Assert.NotNull(revision1Result);
            Assert.Equal(2, revision1Result.Revision);
            var revision2Result = result.FirstOrDefault(r => r.Key == revision2.Key);
            Assert.NotNull(revision2Result);
            Assert.Equal(3, revision2Result.Revision);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoRevisionsExist()
        {
            // Arrange
            var entityId = Guid.NewGuid();

            // Act
            var result = await _fixture.Repository.GetAllAsync(entityId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task RestoreAsync_ThrowsInvalidOperationException_WhenRevisionWasAlreadyRestored()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision = await _fixture.Repository.CreateAsync(entityId);
            await _fixture.Repository.DeleteAsync(revision.Key, entityId);
            await _fixture.Repository.RestoreAsync(revision.Key, entityId);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _fixture.Repository.RestoreAsync(revision.Key, entityId));
        }

        [Fact]
        public async Task GetHistoricAsync_ReturnsCorrectRevision()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision = await _fixture.Repository.CreateAsync(entityId);
            var pointInTime = DateTime.UtcNow.AddMinutes(1);
            var updateResult = await _fixture.Repository.UpdateAsync(revision.Key, Guid.NewGuid());

            // Act
            var result = await _fixture.Repository.GetHistoricAsync(revision.Key, pointInTime);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(revision.Key, result.Key);
            Assert.Equal(updateResult.EntityId, result.EntityId);
        }

        [Fact]
        public async Task GetAllByEntityIdAsync_ReturnsCorrectRevisions()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var revision1 = await _fixture.Repository.CreateAsync(entityId);
            var revision2 = await _fixture.Repository.DeleteAsync(revision1.Key);

            // Act
            var result = await _fixture.Repository.GetAllByEntityIdAsync(entityId);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, r => r.Key == revision1.Key);
            Assert.Contains(result, r => r.Key == revision2.Key);
        }
    }
}