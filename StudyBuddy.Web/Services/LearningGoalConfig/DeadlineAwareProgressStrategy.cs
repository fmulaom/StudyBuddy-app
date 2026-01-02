using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.LearningGoalConfig
{
    public class DeadlineAwareProgressStrategy : ILearningGoalProgressStrategy
    {
        public int CalculateProgress(LearningGoal goal)
        {
            if (goal.TargetDate <= DateTime.Today && goal.Progress < 100)
            {
                return Math.Min(100, goal.Progress + 10);
            }

            return goal.Progress;
        }
    }
}
