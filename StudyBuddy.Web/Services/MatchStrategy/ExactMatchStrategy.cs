using StudyBuddy.Web.Models.Enums;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.MatchStrategy
{
    public class ExactMatchStrategy : IMatchingStrategy
    {
        private readonly IRepository<StudyPartner> _repo;
        public MatchingMode Mode => MatchingMode.ExactMatch;

        public ExactMatchStrategy(IRepository<StudyPartner> repo) => _repo = repo;

        public async Task<IEnumerable<StudyPartner>> MatchAsync(string userId, MatchingCriteria criteria)
        {
            var currentUser = await _repo.GetWhereAsync(x => x.UserId == userId);
            if (!currentUser.Any()) return Enumerable.Empty<StudyPartner>();

            var matches = await _repo.GetWhereAsync(x =>
                x.Subject == criteria.Subject &&
                x.Faculty == criteria.Faculty &&
                x.Level == criteria.Level &&
                x.UserId != currentUser.First().UserId
            );

            return matches;
        }
    }

}
