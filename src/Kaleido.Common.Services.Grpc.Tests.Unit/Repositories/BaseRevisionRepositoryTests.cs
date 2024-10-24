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
            var updatedRevision = new BaseRevisionEntity { EntityId = entityId };

            // Act
            var result = await _fixture.Repository.UpdateAsync(initialRevision.Key, entityId, updatedRevision);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entityId, result.EntityId);
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
            var entityId = Guid.NewGuid();
            var revision = await _fixture.Repository.CreateAsync(entityId);
            await _fixture.Repository.UpdateAsync(revision.Key, entityId);

            // Act
            var result = await _fixture.Repository.GetAllAsync(revision.Key);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
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
    }
}
