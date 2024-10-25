using Kaleido.Common.Services.Grpc.Models;
using Kaleido.Common.Services.Grpc.Tests.Unit.Repositories.Fixture;

namespace Kaleido.Common.Services.Grpc.Tests.Unit.Repositories
{
    public class BaseEntityRepositoryTests : IClassFixture<BaseEntityRepositoryFixture>
    {
        private readonly BaseEntityRepositoryFixture _fixture;

        public BaseEntityRepositoryTests(BaseEntityRepositoryFixture fixture)
        {
            _fixture = fixture;
            fixture.ResetDatabase();
        }

        [Fact]
        public async Task CreateAsync_CreatesEntity()
        {
            // Arrange
            var entity = new BaseEntity();

            // Act
            var result = await _fixture.Repository.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _fixture.Repository.CreateAsync(null!));
        }

        [Fact]
        public async Task GetAsync_ReturnsEntity()
        {
            // Arrange
            var entity = new BaseEntity();
            var createResult = await _fixture.Repository.CreateAsync(entity);

            // Act
            var result = await _fixture.Repository.GetAsync(createResult.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task GetAsync_ReturnsNull_WhenEntityDoesNotExist()
        {
            // Act
            var result = await _fixture.Repository.GetAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            // Arrange
            var entity1 = new BaseEntity();
            var entity2 = new BaseEntity();
            await _fixture.Repository.CreateAsync(entity1);
            await _fixture.Repository.CreateAsync(entity2);

            // Act
            var result = await _fixture.Repository.GetAllAsync();

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task FindAsync_ReturnsEntity_WhenExists()
        {
            // Arrange
            var createResult = await _fixture.Repository.CreateAsync(new BaseEntity());

            // Act
            var result = await _fixture.Repository.FindAsync(e => e.Id == createResult.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createResult.Id, result.Id);
        }

        [Fact]
        public async Task FindAsync_ReturnsNull_WhenNotFound()
        {
            // Act
            var result = await _fixture.Repository.FindAsync(e => e.Id == Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindAllAsync_ReturnsMatchingEntities()
        {
            // Arrange
            var entity1 = new BaseEntity();
            var entity2 = new BaseEntity();
            var createEntity1 = await _fixture.Repository.CreateAsync(entity1);
            await _fixture.Repository.CreateAsync(entity2);

            // Act
            var result = await _fixture.Repository.FindAllAsync(e => e.Id == createEntity1.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal(createEntity1.Id, result.First().Id);
        }

        [Fact]
        public async Task FindAllAsync_ReturnsEmptyForNoMatches()
        {
            // Arrange
            await _fixture.Repository.CreateAsync(new BaseEntity());

            // Act
            var result = await _fixture.Repository.FindAllAsync(e => e.Id.ToString().Contains("@"));

            // Assert
            Assert.Empty(result);
        }
    }
}

