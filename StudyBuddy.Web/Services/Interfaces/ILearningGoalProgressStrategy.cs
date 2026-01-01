using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services.Interfaces
{
    public interface ILearningGoalProgressStrategy
    {
        int CalculateProgress(LearningGoal goal);
    }
}
