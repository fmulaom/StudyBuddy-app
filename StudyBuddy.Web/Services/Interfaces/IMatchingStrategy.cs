using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.Enums;

namespace StudyBuddy.Web.Services.Interfaces
{
    public interface IMatchingStrategy
    {
        MatchingMode Mode { get; }
        Task<IEnumerable<StudyPartner>> MatchAsync(string userId, MatchingCriteria criteria);
    }
}
