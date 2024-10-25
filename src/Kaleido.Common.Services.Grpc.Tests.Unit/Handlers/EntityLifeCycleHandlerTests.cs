using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Tests.Unit.Handlers.Fixtures;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

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
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
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
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenEntityDoesNotExist()
        {
            // Arrange
            var updatedEntity = new BaseEntity() { Id = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
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
    }
}

