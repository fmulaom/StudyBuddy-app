using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.LearningGoalConfig;


namespace StudyBuddy.Test
{
    public class LearningGoalConfigTests
    {
        [Fact]
        public void Instance_AlwaysReturnsSameObject()
        {
            var a = LearningGoalConfig.Instance;
            var b = LearningGoalConfig.Instance;

            Assert.Same(a, b);
        }

        [Fact]
        public void DefaultValues_AreConfiguredCorrectly()
        {
            var cfg = LearningGoalConfig.Instance;

            Assert.Equal(0, cfg.DefaultProgress);
            Assert.Equal(7, cfg.DefaultDaysFromToday);
        }
        [Fact]
        public void LearningGoal_CreatedAt_CanBeSet()
        {
           
            var goal = new LearningGoal();
            var expectedDate = DateTime.UtcNow;

          
            goal.CreatedAt = expectedDate;


            Assert.Equal(expectedDate, goal.CreatedAt);
        }
        [Fact]
        public void LearningGoal_CreatedAt_HasDefaultValue()
        {
            var goal = new LearningGoal();

            Assert.Equal(default(DateTime), goal.CreatedAt); 
        }

    }
}
