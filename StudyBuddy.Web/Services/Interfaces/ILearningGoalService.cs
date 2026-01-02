using StudyBuddy.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyBuddy.Web.Services.Interfaces
{
    public interface ILearningGoalService
    {
        Task<IEnumerable<LearningGoal>> GetGoalsForUserAsync(string userId);
        Task<LearningGoal?> GetGoalForUserAsync(int id, string userId);
        Task CreateGoalAsync(LearningGoal goal);
        Task UpdateGoalAsync(LearningGoal goal, string userId);
        Task DeleteGoalAsync(int id, string userId);
    }
}
