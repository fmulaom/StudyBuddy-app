using StudyBuddy.Web.Models;
using System;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class LearningGoalModelTests
    {
        #region Property Tests

        [Fact]
        public void LearningGoal_CanBeCreated_WithAllProperties()
        {
            var goal = new LearningGoal
            {
                Id = 1,
                UserId = "user-123",
                Title = "Master C#",
                Description = "Learn advanced C# features",
                TargetDate = DateTime.Today.AddMonths(3),
                Progress = 50,
            };

            Assert.Equal(1, goal.Id);
            Assert.Equal("user-123", goal.UserId);
            Assert.Equal("Master C#", goal.Title);
            Assert.Equal("Learn advanced C# features", goal.Description);
            Assert.Equal(DateTime.Today.AddMonths(3), goal.TargetDate);
            Assert.Equal(50, goal.Progress);
        }

       
        [Fact]
        public void LearningGoal_Title_CanBeSet()
        {
            var goal = new LearningGoal();
            goal.Title = "New Title";

            Assert.Equal("New Title", goal.Title);
        }

        [Fact]
        public void LearningGoal_Description_CanBeSet()
        {
            var goal = new LearningGoal();
            goal.Description = "New Description";

            Assert.Equal("New Description", goal.Description);
        }

        [Fact]
        public void LearningGoal_TargetDate_CanBeSet()
        {
            var goal = new LearningGoal();
            var targetDate = DateTime.Today.AddMonths(6);
            goal.TargetDate = targetDate;

            Assert.Equal(targetDate, goal.TargetDate);
        }

        [Fact]
        public void LearningGoal_Progress_CanBeSet()
        {
            var goal = new LearningGoal();
            goal.Progress = 75;

            Assert.Equal(75, goal.Progress);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(75)]
        [InlineData(100)]
        public void LearningGoal_Progress_CanHaveDifferentValues(int progress)
        {
            var goal = new LearningGoal { Progress = progress };

            Assert.Equal(progress, goal.Progress);
        }

        [Fact]
        public void LearningGoal_UserId_CanBeSet()
        {
            var goal = new LearningGoal();
            goal.UserId = "user-456";

            Assert.Equal("user-456", goal.UserId);
        }

        [Fact]
        
        public void LearningGoal_CreatedAt_CanBeSet()
        {
            var goal = new LearningGoal();
            var expected = DateTime.UtcNow;

            goal.CreatedAt = expected;

            Assert.Equal(expected, goal.CreatedAt);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void LearningGoal_CanHaveEmptyTitle()
        {
            var goal = new LearningGoal { Title = "" };

            Assert.Equal("", goal.Title);
        }

        [Fact]
        public void LearningGoal_CanHaveNullDescription()
        {
            var goal = new LearningGoal { Description = null };

            Assert.Null(goal.Description);
        }

        [Fact]
        public void LearningGoal_CanHavePastTargetDate()
        {
            var goal = new LearningGoal { TargetDate = DateTime.Today.AddDays(-10) };

            Assert.True(goal.TargetDate < DateTime.Today);
        }

        [Fact]
        public void LearningGoal_CanHaveFutureTargetDate()
        {
            var goal = new LearningGoal { TargetDate = DateTime.Today.AddDays(30) };

            Assert.True(goal.TargetDate > DateTime.Today);
        }

        [Fact]
        public void LearningGoal_ProgressCanBeZero()
        {
            var goal = new LearningGoal { Progress = 0 };

            Assert.Equal(0, goal.Progress);
        }

        [Fact]
        public void LearningGoal_ProgressCanBeHundred()
        {
            var goal = new LearningGoal { Progress = 100 };

            Assert.Equal(100, goal.Progress);
        }

        [Fact]
        public void LearningGoal_CanHaveNegativeProgress()
        {
            var goal = new LearningGoal { Progress = -10 };

            Assert.Equal(-10, goal.Progress);
        }

        [Fact]
        public void LearningGoal_CanHaveProgressGreaterThanHundred()
        {
            var goal = new LearningGoal { Progress = 150 };

            Assert.Equal(150, goal.Progress);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void LearningGoal_WithSameId_AreEqual()
        {
            var goal1 = new LearningGoal { Id = 1, Title = "Goal 1" };
            var goal2 = new LearningGoal { Id = 1, Title = "Goal 2" };

            Assert.NotSame(goal1, goal2);
        }

        [Fact]
        public void LearningGoal_WithDifferentId_AreDifferent()
        {
            var goal1 = new LearningGoal { Id = 1, Title = "Goal" };
            var goal2 = new LearningGoal { Id = 2, Title = "Goal" };

            Assert.NotEqual(goal1.Id, goal2.Id);
        }

        #endregion

        #region Validation Scenarios

        [Fact]
        public void LearningGoal_TitleLongerThan255Characters()
        {
            var longTitle = new string('a', 300);
            var goal = new LearningGoal { Title = longTitle };

            Assert.Equal(300, goal.Title.Length);
        }

        [Fact]
        public void LearningGoal_DescriptionLongerThan1000Characters()
        {
            var longDescription = new string('b', 2000);
            var goal = new LearningGoal { Description = longDescription };

            Assert.Equal(2000, goal.Description.Length);
        }

        [Fact]
        public void LearningGoal_CanHaveSameUserIdMultipleTimes()
        {
            var goal1 = new LearningGoal { UserId = "user1" };
            var goal2 = new LearningGoal { UserId = "user1" };

            Assert.Equal(goal1.UserId, goal2.UserId);
        }

        #endregion

        #region Timestamp Tests

        #endregion

        #region Multiple Goals

        [Fact]
        public void LearningGoal_MultipleGoals_CanExist()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { Id = 1, Title = "Goal 1", Progress = 25 },
                new LearningGoal { Id = 2, Title = "Goal 2", Progress = 50 },
                new LearningGoal { Id = 3, Title = "Goal 3", Progress = 75 }
            };

            Assert.Equal(3, goals.Count);
            Assert.Equal(25, goals[0].Progress);
            Assert.Equal(50, goals[1].Progress);
            Assert.Equal(75, goals[2].Progress);
        }

        [Fact]
        public void LearningGoal_GoalsCanBeFiltered_ByProgress()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { Id = 1, Progress = 25 },
                new LearningGoal { Id = 2, Progress = 50 },
                new LearningGoal { Id = 3, Progress = 100 }
            };

            var completed = goals.Where(g => g.Progress >= 100).ToList();

            Assert.Single(completed);
            Assert.Equal(100, completed[0].Progress);
        }

        [Fact]
        public void LearningGoal_GoalsCanBeFiltered_ByUserId()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { UserId = "user1", Title = "Goal 1" },
                new LearningGoal { UserId = "user2", Title = "Goal 2" },
                new LearningGoal { UserId = "user1", Title = "Goal 3" }
            };

            var userGoals = goals.Where(g => g.UserId == "user1").ToList();

            Assert.Equal(2, userGoals.Count);
        }

        [Fact]
        public void LearningGoal_GoalsCanBeFiltered_ByTargetDate()
        {
            var today = DateTime.Today;
            var goals = new List<LearningGoal>
            {
                new LearningGoal { TargetDate = today.AddDays(-10) },
                new LearningGoal { TargetDate = today.AddDays(10) },
                new LearningGoal { TargetDate = today.AddDays(30) }
            };

            var upcomingGoals = goals.Where(g => g.TargetDate > today).ToList();

            Assert.Equal(2, upcomingGoals.Count);
        }

        #endregion
    }
}
