using Moq;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.LearningGoalConfig;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace StudyBuddy.Tests.Services
{
    #region LearningGoalFacade Tests

    public class LearningGoalFacadeTests
    {
        private readonly Mock<ILearningGoalService> _mockService;
        private readonly LearningGoalFacade _facade;

        public LearningGoalFacadeTests()
        {
            _mockService = new Mock<ILearningGoalService>();
            _facade = new LearningGoalFacade(_mockService.Object);
        }

        [Fact]
        public async Task GetMyGoalsAsync_ReturnsUserGoals()
        {
            var userId = "user1";
            var goals = new List<LearningGoal>
            {
                new LearningGoal { Id = 1, UserId = userId, Title = "Learn C#", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(30) },
                new LearningGoal { Id = 2, UserId = userId, Title = "Learn ASP.NET", Progress = 30, TargetDate = DateTime.UtcNow.AddDays(60) }
            };

            _mockService.Setup(s => s.GetGoalsForUserAsync(userId)).ReturnsAsync(goals);

            var result = await _facade.GetMyGoalsAsync(userId);

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateMyGoalAsync_SetsUserIdAndCreates()
        {
            var userId = "user1";
            var goal = new LearningGoal { Id = 1, Title = "Learn Docker", Progress = 0, TargetDate = DateTime.UtcNow.AddDays(45) };

            _mockService.Setup(s => s.CreateGoalAsync(It.IsAny<LearningGoal>())).Returns(Task.CompletedTask);

            await _facade.CreateMyGoalAsync(userId, goal);

            Assert.Equal(userId, goal.UserId);
            _mockService.Verify(s => s.CreateGoalAsync(It.IsAny<LearningGoal>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMyGoalAsync_UpdatesGoal()
        {
            var userId = "user1";
            var goal = new LearningGoal { Id = 1, UserId = userId, Title = "Updated", Progress = 75, TargetDate = DateTime.UtcNow.AddDays(20) };

            _mockService.Setup(s => s.UpdateGoalAsync(goal, userId)).Returns(Task.CompletedTask);

            await _facade.UpdateMyGoalAsync(userId, goal);

            _mockService.Verify(s => s.UpdateGoalAsync(goal, userId), Times.Once);
        }

        [Fact]
        public async Task DeleteMyGoalAsync_DeletesGoal()
        {
            var userId = "user1";

            _mockService.Setup(s => s.DeleteGoalAsync(1, userId)).Returns(Task.CompletedTask);

            await _facade.DeleteMyGoalAsync(userId, 1);

            _mockService.Verify(s => s.DeleteGoalAsync(1, userId), Times.Once);
        }
    }

    #endregion

    #region LearningGoalService Tests

    public class LearningGoalServiceTests
    {
        private ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetGoalsForUserAsync_ReturnsUserGoals()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var userId = "user1";
            var goal1 = new LearningGoal { UserId = userId, Title = "Goal 1", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(30) };
            var goal2 = new LearningGoal { UserId = userId, Title = "Goal 2", Progress = 30, TargetDate = DateTime.UtcNow.AddDays(60) };

            context.LearningGoals.Add(goal1);
            context.LearningGoals.Add(goal2);
            await context.SaveChangesAsync();

            var result = await service.GetGoalsForUserAsync(userId);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetGoalsForUserAsync_ReturnsEmpty_WhenNoGoals()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var result = await service.GetGoalsForUserAsync("nonexistent");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGoalForUserAsync_ReturnsGoal_WhenExists()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var userId = "user1";
            var goal = new LearningGoal { UserId = userId, Title = "Goal", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(30) };

            context.LearningGoals.Add(goal);
            await context.SaveChangesAsync();

            var result = await service.GetGoalForUserAsync(goal.Id, userId);

            Assert.NotNull(result);
            Assert.Equal("Goal", result.Title);
        }

        [Fact]
        public async Task GetGoalForUserAsync_ReturnsNull_WhenNotFound()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var result = await service.GetGoalForUserAsync(999, "user1");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateGoalAsync_AddsGoal()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var goal = new LearningGoal { UserId = "user1", Title = "New Goal", Progress = 0, TargetDate = DateTime.UtcNow.AddDays(30) };

            await service.CreateGoalAsync(goal);

            var result = await context.LearningGoals.FirstOrDefaultAsync(g => g.Title == "New Goal");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateGoalAsync_UpdatesGoal()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var userId = "user1";
            var goal = new LearningGoal { UserId = userId, Title = "Old", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(30) };

            context.LearningGoals.Add(goal);
            await context.SaveChangesAsync();

            var updateGoal = new LearningGoal { Id = goal.Id, UserId = userId, Title = "New", Progress = 75, TargetDate = DateTime.UtcNow.AddDays(20) };
            mockStrategy.Setup(s => s.CalculateProgress(It.IsAny<LearningGoal>())).Returns(85);

            await service.UpdateGoalAsync(updateGoal, userId);

            var result = await context.LearningGoals.FirstOrDefaultAsync(g => g.Id == goal.Id);
            Assert.Equal("New", result.Title);
            Assert.Equal(85, result.Progress);
        }

        [Fact]
        public async Task UpdateGoalAsync_DoesNothing_WhenNotFound()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var updateGoal = new LearningGoal { Id = 999, UserId = "user1", Title = "Title", Progress = 50, TargetDate = DateTime.UtcNow };

            await service.UpdateGoalAsync(updateGoal, "user1");

            var count = await context.LearningGoals.CountAsync();
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task DeleteGoalAsync_DeletesGoal()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var userId = "user1";
            var goal = new LearningGoal { UserId = userId, Title = "Goal", Progress = 50, TargetDate = DateTime.UtcNow };

            context.LearningGoals.Add(goal);
            await context.SaveChangesAsync();

            await service.DeleteGoalAsync(goal.Id, userId);

            var result = await context.LearningGoals.FirstOrDefaultAsync(g => g.Id == goal.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteGoalAsync_DoesNothing_WhenNotFound()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var countBefore = await context.LearningGoals.CountAsync();
            await service.DeleteGoalAsync(999, "user1");
            var countAfter = await context.LearningGoals.CountAsync();

            Assert.Equal(countBefore, countAfter);
        }

        [Theory]
        [InlineData("Learn Python")]
        [InlineData("Learn Kubernetes")]
        [InlineData("Learn React")]
        public async Task CreateGoalAsync_WorksWithDifferentTitles(string title)
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var goal = new LearningGoal { UserId = "user1", Title = title, Progress = 0, TargetDate = DateTime.UtcNow.AddDays(30) };

            await service.CreateGoalAsync(goal);

            var result = await context.LearningGoals.FirstOrDefaultAsync(g => g.Title == title);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetGoalsForUserAsync_OrdersByTargetDate()
        {
            var context = CreateContext();
            var mockStrategy = new Mock<ILearningGoalProgressStrategy>();
            var service = new LearningGoalService(context, mockStrategy.Object);

            var userId = "user1";
            var goal1 = new LearningGoal { UserId = userId, Title = "Goal 1", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(60) };
            var goal2 = new LearningGoal { UserId = userId, Title = "Goal 2", Progress = 30, TargetDate = DateTime.UtcNow.AddDays(30) };

            context.LearningGoals.Add(goal1);
            context.LearningGoals.Add(goal2);
            await context.SaveChangesAsync();

            var result = await service.GetGoalsForUserAsync(userId);
            var resultList = result.ToList();

            Assert.True(resultList[0].TargetDate <= resultList[1].TargetDate);
        }
    }

    #endregion

    #region SimpleProgressStrategy Tests

    public class SimpleProgressStrategyTests
    {
        [Fact]
        public void CalculateProgress_ReturnsGoalProgress()
        {
            var strategy = new SimpleProgressStrategy();
            var goal = new LearningGoal { Progress = 75, TargetDate = DateTime.UtcNow };

            var result = strategy.CalculateProgress(goal);

            Assert.Equal(75, result);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void CalculateProgress_ReturnsVariousProgressValues(int progressValue)
        {
            var strategy = new SimpleProgressStrategy();
            var goal = new LearningGoal { Progress = progressValue, TargetDate = DateTime.UtcNow };

            var result = strategy.CalculateProgress(goal);

            Assert.Equal(progressValue, result);
        }
    }

    #endregion

    #region DeadlineAwareProgressStrategy Tests

    public class DeadlineAwareProgressStrategyTests
    {
        [Fact]
        public void CalculateProgress_ReturnsValidValue_WhenDeadlineApproaches()
        {
            var strategy = new DeadlineAwareProgressStrategy();
            var goal = new LearningGoal { Progress = 50, TargetDate = DateTime.UtcNow.AddDays(1) };

            var result = strategy.CalculateProgress(goal);

            Assert.True(result >= 0 && result <= 100);
        }

        [Fact]
        public void CalculateProgress_ReturnsValidValue_WhenDeadlineIsFar()
        {
            var strategy = new DeadlineAwareProgressStrategy();
            var goal = new LearningGoal { Progress = 30, TargetDate = DateTime.UtcNow.AddDays(60) };

            var result = strategy.CalculateProgress(goal);

            Assert.True(result >= 0 && result <= 100);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        public void CalculateProgress_WorksWithVariousDeadlines(int daysFromNow)
        {
            var strategy = new DeadlineAwareProgressStrategy();
            var goal = new LearningGoal { Progress = 50, TargetDate = DateTime.UtcNow.AddDays(daysFromNow) };

            var result = strategy.CalculateProgress(goal);

            Assert.True(result >= 0 && result <= 100);
        }
    }

    #endregion
}
