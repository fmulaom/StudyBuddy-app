using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.Enums;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.MatchStrategy;

namespace StudyBuddy.Web.Services.ServicesImplementation
{
    /// <summary>
    /// SOLID - S (Single Responsibility):
    /// Samo upravlja podudaranjem partnera. Notifikacije su u INotificationService.
    /// 
    /// SOLID - DIP (Dependency Inversion):
    /// Nema pravljenja objekta s 'new', sve se injektira kroz konstruktor.
    /// </summary>
    public class PartnerMatchingService : IPartnerMatchingService
    {
        private readonly IMatchingStrategyFactory _strategyFactory;
        private readonly IRepository<StudyPartner> _partnerRepository;
        private readonly ILogger<PartnerMatchingService> _logger;

        public PartnerMatchingService(
            IMatchingStrategyFactory strategyFactory,
            ILogger<PartnerMatchingService> logger,
            IRepository<StudyPartner> partnerRepository)
        {
            _strategyFactory = strategyFactory;
            _logger = logger;
            _partnerRepository = partnerRepository;
        }

        public async Task<IReadOnlyList<StudyPartner>> GetAutomaticMatchesAsync(string userId)
        {
            var criteria = new MatchingCriteria { Mode = MatchingMode.Automatic };
            var strategy = _strategyFactory.GetStrategy(MatchingMode.ExactMatch);
            var matches = await strategy.MatchAsync(userId, criteria);
            return matches.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<StudyPartner>> SearchPartnersAsync(string subject, string faculty, string level)
        {
            var criteria = new MatchingCriteria
            {
                Subject = subject,
                Faculty = faculty,
                Level = level,
                Mode = MatchingMode.FacultyPriority
            };

            var strategy = _strategyFactory.GetStrategy(criteria.Mode);
            var matches = await strategy.MatchAsync(null, criteria);
            return matches.ToList().AsReadOnly();
        }


        /// <summary>
        /// SOLID - S: Samo označava omiljene.
        /// </summary>
        public async Task MarkFavoriteAsync(string userId, int partnerId)
        {
            var partner = await _partnerRepository.GetByIdAsync(partnerId);
            if (partner?.UserId != userId)
            {
                throw new UnauthorizedAccessException("Možeš označiti samo svoje omiljene.");
            }

            partner.IsFavorite = true;
            await _partnerRepository.UpdateAsync(partner);
        }
    }
}
