using Microsoft.EntityFrameworkCore;
using Moq;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Repositories;
using Xunit;

namespace StudyBuddy.Test.Services
{
    public class RepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
        private readonly ApplicationDbContext _context;
        private readonly Repository<LearningGoal> _repository;

        public RepositoryTests()
        {
            // Koristi in-memory bazu umjesto mock-a
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_dbContextOptions);
            _repository = new Repository<LearningGoal>(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity()
        {
            var goal = new LearningGoal { Id = 1, Title = "Learn C#", Progress = 0, TargetDate = DateTime.UtcNow.AddDays(30) };
            await _context.LearningGoals.AddAsync(goal);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Learn C#", result.Title);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEntities()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { Title = "Goal 1", Progress = 0, TargetDate = DateTime.UtcNow },
                new LearningGoal { Title = "Goal 2", Progress = 50, TargetDate = DateTime.UtcNow }
            };
            await _context.LearningGoals.AddRangeAsync(goals);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAsync_AddsEntityAndSaves()
        {
            var goal = new LearningGoal { Title = "New Goal", Progress = 0, TargetDate = DateTime.UtcNow };

            await _repository.AddAsync(goal);

            var result = await _context.LearningGoals.FirstOrDefaultAsync(x => x.Title == "New Goal");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEntityAndSaves()
        {
            var goal = new LearningGoal { Title = "Old Title", Progress = 0, TargetDate = DateTime.UtcNow };
            await _context.LearningGoals.AddAsync(goal);
            await _context.SaveChangesAsync();

            goal.Title = "New Title";
            await _repository.UpdateAsync(goal);

            var result = await _context.LearningGoals.FirstOrDefaultAsync(x => x.Id == goal.Id);
            Assert.Equal("New Title", result.Title);
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntityAndSaves()
        {
            var goal = new LearningGoal { Title = "Goal to Delete", Progress = 0, TargetDate = DateTime.UtcNow };
            await _context.LearningGoals.AddAsync(goal);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(goal);

            var result = await _context.LearningGoals.FirstOrDefaultAsync(x => x.Id == goal.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveChangesAsync_CallsContextSaveChanges()
        {
            var goal = new LearningGoal { Title = "Test", Progress = 0, TargetDate = DateTime.UtcNow };
            await _context.LearningGoals.AddAsync(goal);
            await _repository.SaveChangesAsync();

            var result = await _context.LearningGoals.FirstOrDefaultAsync(x => x.Title == "Test");
            Assert.NotNull(result);
        }
    }
}
