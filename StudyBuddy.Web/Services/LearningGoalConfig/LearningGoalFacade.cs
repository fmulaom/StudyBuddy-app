using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.LearningGoalConfig
{
    public class LearningGoalFacade : ILearningGoalFacade
    {
        private readonly ILearningGoalService _service;

        public LearningGoalFacade(ILearningGoalService service)
        {
            _service = service;
        }

        public Task<IEnumerable<LearningGoal>> GetMyGoalsAsync(string userId)
            => _service.GetGoalsForUserAsync(userId);

        public async Task CreateMyGoalAsync(string userId, LearningGoal goal)
        {
            goal.UserId = userId;
            await _service.CreateGoalAsync(goal);
        }

        public Task UpdateMyGoalAsync(string userId, LearningGoal goal)
            => _service.UpdateGoalAsync(goal, userId);

        public Task DeleteMyGoalAsync(string userId, int id)
            => _service.DeleteGoalAsync(id, userId);
    }
}
