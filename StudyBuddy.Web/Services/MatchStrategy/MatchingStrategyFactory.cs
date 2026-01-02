using StudyBuddy.Web.Models.Enums;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.MatchStrategy
{
    public interface IMatchingStrategyFactory
    {
        IMatchingStrategy GetStrategy(MatchingMode mode);
    }

    public class MatchingStrategyFactory : IMatchingStrategyFactory
    {
        private readonly IEnumerable<IMatchingStrategy> _strategies;

        public MatchingStrategyFactory(IEnumerable<IMatchingStrategy> strategies)
        {
            _strategies = strategies;
        }

        public IMatchingStrategy GetStrategy(MatchingMode mode)
        {
            return _strategies.FirstOrDefault(s => s.Mode == mode)
                   ?? throw new InvalidOperationException($"No strategy for {mode}");
        }
    }

}
