using System;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.LearningGoalConfig;
namespace StudyBuddy.Test

{
    public class UnitTest1
    {
        private readonly ILearningGoalProgressStrategy _strategy = new DeadlineAwareProgressStrategy();

        [Fact]
        public void PastDeadline_IncreasesProgressBy10()
        {
            var goal = new LearningGoal
            {
                Progress = 50,
                TargetDate = DateTime.Today.AddDays(-1)
            };

            var result = _strategy.CalculateProgress(goal);

            Assert.Equal(60, result);
        }

        [Fact]
        public void FutureDeadline_KeepsProgressSame()
        {
            var goal = new LearningGoal
            {
                Progress = 50,
                TargetDate = DateTime.Today.AddDays(5)
            };

            var result = _strategy.CalculateProgress(goal);

            Assert.Equal(50, result);
        }

        [Fact]
        public void PastDeadline_DoesNotExceed100()
        {
            var goal = new LearningGoal
            {
                Progress = 95,
                TargetDate = DateTime.Today.AddDays(-1)
            };

            var result = _strategy.CalculateProgress(goal);

            Assert.Equal(100, result);
        }
    }
}
