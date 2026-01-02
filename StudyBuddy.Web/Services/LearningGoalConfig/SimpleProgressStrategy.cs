using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.LearningGoalConfig
{
    public class SimpleProgressStrategy : ILearningGoalProgressStrategy
    {
        public int CalculateProgress(LearningGoal goal)
        {
            return goal.Progress;
        }
    }
}
