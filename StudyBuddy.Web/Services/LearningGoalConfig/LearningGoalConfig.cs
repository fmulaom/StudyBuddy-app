namespace StudyBuddy.Web.Services.LearningGoalConfig
{
    public sealed  class LearningGoalConfig
    {
        private static LearningGoalConfig? _instance;
        private LearningGoalConfig() { }

        public static LearningGoalConfig Instance
            => _instance ??= new LearningGoalConfig();

        public int DefaultProgress => 0;
        public int DefaultDaysFromToday => 7;
    }
}
