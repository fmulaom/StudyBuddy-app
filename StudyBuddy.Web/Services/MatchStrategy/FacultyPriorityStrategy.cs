using StudyBuddy.Web.Models.Enums;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.MatchStrategy
{
    public class FacultyPriorityStrategy : IMatchingStrategy
    {
        private readonly IRepository<StudyPartner> _repo;
        public MatchingMode Mode => MatchingMode.FacultyPriority;

        public FacultyPriorityStrategy(IRepository<StudyPartner> repo) => _repo = repo;

        public async Task<IEnumerable<StudyPartner>> MatchAsync(string userId, MatchingCriteria criteria)
        {
            var matches = await _repo.GetWhereAsync(x =>
                x.Subject.Contains(criteria.Subject) &&
                x.UserId != userId
            );

            return matches.OrderBy(x => x.Faculty == criteria.Faculty ? 0 : 1)
                         .ThenBy(x => Math.Abs(GetLevelScore(x.Level, criteria.Level)));
        }

        private int GetLevelScore(string level1, string level2)
        {
            var scores = new Dictionary<string, int> { { "Beginner", 1 }, { "Intermediate", 2 }, { "Advanced", 3 } };
            return Math.Abs(scores[level1] - scores[level2]);
        }
    }

}
