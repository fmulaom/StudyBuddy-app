using StudyBuddy.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyBuddy.Web.Services.Interfaces
{
    // STRUCTURAL PATTERN – FACADE
    public interface ILearningGoalFacade
    {
        Task<IEnumerable<LearningGoal>> GetMyGoalsAsync(string userId);
        Task CreateMyGoalAsync(string userId, LearningGoal goal);
        Task UpdateMyGoalAsync(string userId, LearningGoal goal);
        Task DeleteMyGoalAsync(string userId, int id);
    }
}
