using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Repositories;
using Xunit;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Threading.Tasks;

namespace StudyBuddy.Test.Services.Repositories
{
    public class RepositoryTests : IAsyncLifetime
    {
        private ApplicationDbContext _context;
        private Repository<StudyPartner> _repository;

        public async Task InitializeAsync()
        {
            // Setup in-memory database za testove
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _context = new ApplicationDbContext(options);
            _repository = new Repository<StudyPartner>(_context);

            // Inicijalizacija baze
            await _context.Database.EnsureCreatedAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity_WhenIdExists()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetByIdAsync(partner.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partner.Id, result.Id);
            Assert.Equal("Math", result.Subject);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public async Task GetByIdAsync_WorksWithDifferentIds(int id)
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetByIdAsync(id);

            // Assert
            if (id == partner.Id)
                Assert.NotNull(result);
            else
                Assert.Null(result);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            // Arrange
            var partners = new List<StudyPartner>
            {
                new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user3", Subject = "Biology", Faculty = "Medicine", Level = "Beginner", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };

            foreach (var partner in partners)
                await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoEntities()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsIReadOnlyList()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<StudyPartner>>(result);
        }

        #endregion

        #region GetWhereAsync Tests

        [Fact]
        public async Task GetWhereAsync_ReturnFilteredEntities_WhenPredicateMatches()
        {
            // Arrange
            var partners = new List<StudyPartner>
            {
                new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user3", Subject = "Math", Faculty = "Science", Level = "Beginner", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };

            foreach (var partner in partners)
                await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetWhereAsync(p => p.Subject == "Math");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal("Math", p.Subject));
        }

        [Fact]
        public async Task GetWhereAsync_ReturnsEmptyList_WhenNoMatch()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetWhereAsync(p => p.Subject == "NonExistent");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWhereAsync_FiltersByMultipleConditions()
        {
            // Arrange
            var partners = new List<StudyPartner>
            {
                new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user2", Subject = "Math", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user3", Subject = "Physics", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };

            foreach (var partner in partners)
                await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetWhereAsync(p => p.Subject == "Math" && p.Level == "Advanced");

            // Assert
            Assert.Single(result);
            Assert.Equal("user1", result.First().UserId);
        }

        [Fact]
        public async Task GetWhereAsync_FiltersByUserId()
        {
            // Arrange
            var partners = new List<StudyPartner>
            {
                new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };

            foreach (var partner in partners)
                await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetWhereAsync(p => p.UserId == "user1");

            // Assert
            Assert.Single(result);
            Assert.Equal("user1", result.First().UserId);
        }

        [Theory]
        [InlineData("Math")]
        [InlineData("Physics")]
        [InlineData("Biology")]
        public async Task GetWhereAsync_WorksWithDifferentSubjects(string subject)
        {
            // Arrange
            var partners = new List<StudyPartner>
            {
                new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };

            foreach (var partner in partners)
                await _repository.AddAsync(partner);

            // Act
            var result = await _repository.GetWhereAsync(p => p.Subject == subject);

            // Assert
            if (subject == "Math" || subject == "Physics")
                Assert.Single(result);
            else
                Assert.Empty(result);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_AddsEntityToDatabase()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(partner);

            // Assert
            var result = await _repository.GetByIdAsync(partner.Id);
            Assert.NotNull(result);
            Assert.Equal("user1", result.UserId);
        }

        [Fact]
        public async Task AddAsync_SetsPrimaryKeyAfterAdd()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(partner);

            // Assert
            Assert.NotEqual(0, partner.Id);
        }

        [Fact]
        public async Task AddAsync_AllowsMultipleEntities()
        {
            // Arrange
            var partner1 = new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow };
            var partner2 = new StudyPartner { UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow };

            // Act
            await _repository.AddAsync(partner1);
            await _repository.AddAsync(partner2);

            // Assert
            var all = await _repository.GetAllAsync();
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public async Task AddAsync_PersistsToDatabase()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await _repository.AddAsync(partner);
            var id = partner.Id;

            // Assert - fetch new repository instance to verify persistence
            var newRepository = new Repository<StudyPartner>(_context);
            var result = await newRepository.GetByIdAsync(id);
            Assert.NotNull(result);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_UpdatesEntity()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);

            // Act
            partner.Subject = "Physics";
            partner.Level = "Intermediate";
            await _repository.UpdateAsync(partner);

            // Assert
            var result = await _repository.GetByIdAsync(partner.Id);
            Assert.Equal("Physics", result.Subject);
            Assert.Equal("Intermediate", result.Level);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesOnlyChangedFields()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);
            var originalUserId = partner.UserId;

            // Act
            partner.Subject = "Physics";
            await _repository.UpdateAsync(partner);

            // Assert
            var result = await _repository.GetByIdAsync(partner.Id);
            Assert.Equal(originalUserId, result.UserId);
            Assert.Equal("Physics", result.Subject);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesIsFavoriteFlag()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);

            // Act
            partner.IsFavorite = true;
            await _repository.UpdateAsync(partner);

            // Assert
            var result = await _repository.GetByIdAsync(partner.Id);
            Assert.True(result.IsFavorite);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_RemovesEntityFromDatabase()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(partner);
            var id = partner.Id;

            // Act
            await _repository.DeleteAsync(partner);

            // Assert
            var result = await _repository.GetByIdAsync(id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_AllowsDeleteMultipleEntities()
        {
            // Arrange
            var partner1 = new StudyPartner { UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow };
            var partner2 = new StudyPartner { UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow };

            await _repository.AddAsync(partner1);
            await _repository.AddAsync(partner2);

            // Act
            await _repository.DeleteAsync(partner1);

            // Assert
            var all = await _repository.GetAllAsync();
            Assert.Single(all);
            Assert.Equal(partner2.Id, all.First().Id);
        }

        #endregion

        #region SaveChangesAsync Tests

        [Fact]
        public async Task SaveChangesAsync_SavesChangesToDatabase()
        {
            // Arrange
            var partner = new StudyPartner
            {
                UserId = "user1",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Set<StudyPartner>().Add(partner);

            // Act
            await _repository.SaveChangesAsync();

            // Assert
            var result = await _repository.GetByIdAsync(partner.Id);
            Assert.NotNull(result);
        }

        #endregion
    }
}
