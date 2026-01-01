using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.LearningGoalConfig
{
    public class LearningGoalService : ILearningGoalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILearningGoalProgressStrategy _progressStrategy;

        public LearningGoalService(
            ApplicationDbContext context,
            ILearningGoalProgressStrategy progressStrategy)
        {
            _context = context;
            _progressStrategy = progressStrategy;
        }

        public async Task<IEnumerable<LearningGoal>> GetGoalsForUserAsync(string userId)
        {
            return await _context.LearningGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<LearningGoal?> GetGoalForUserAsync(int id, string userId)
        {
            return await _context.LearningGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        }

        public async Task CreateGoalAsync(LearningGoal goal)
        {
            _context.LearningGoals.Add(goal);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGoalAsync(LearningGoal goal, string userId)
        {
            var existing = await GetGoalForUserAsync(goal.Id, userId);
            if (existing == null) return;

            existing.Title = goal.Title;
            existing.Description = goal.Description;
            existing.TargetDate = goal.TargetDate;

            // STRATEGY se koristi ovdje.
            existing.Progress = _progressStrategy.CalculateProgress(goal);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(int id, string userId)
        {
            var existing = await GetGoalForUserAsync(id, userId);
            if (existing == null) return;

            _context.LearningGoals.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
