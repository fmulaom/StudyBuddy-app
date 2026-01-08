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
    }
}
