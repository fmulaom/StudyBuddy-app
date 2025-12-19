using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

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
        private readonly IRepository<StudyPartner> _partnerRepository;
        private readonly ILogger<PartnerMatchingService> _logger;

        // SOLID - DIP: Konstruktor prima interfejse, ne konkretne klase
        public PartnerMatchingService(IRepository<StudyPartner> partnerRepository, ILogger<PartnerMatchingService> logger)
        {
            _partnerRepository = partnerRepository ?? throw new ArgumentNullException(nameof(partnerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// SOLID - S: Ova metoda ima jednu odgovornost - pronalazak podudaranja.
        /// Algoritam je odvojen u zasebnu metodu za lakše testiranje i održavanje.
        /// </summary>
        public async Task<IReadOnlyList<StudyPartner>> GetAutomaticMatchesAsync(string userId)
        {
            try
            {
                // SOLID - S: Pronalazi trenutnog korisnika
                var currentPartner = await _partnerRepository.GetWhereAsync(x => x.UserId == userId);
                if (!currentPartner.Any())
                {
                    _logger.LogWarning($"Partner s UserID {userId} nije pronađen.");
                    return new List<StudyPartner>();
                }

                var partner = currentPartner.First();

                // SOLID - S: Pronalazi samo potencijalne podudarke
                var matches = await _partnerRepository.GetWhereAsync(x =>
                    x.Id != partner.Id &&                           // Nije sам sebi
                    x.Subject == partner.Subject &&                 // Isti predmet
                    x.Level == partner.Level &&                     // Ista razina
                    x.Faculty == partner.Faculty                    // Isti fakultet
                );

                _logger.LogInformation($"Pronađeno {matches.Count} podudaranja za {userId}.");
                return matches;
            }
            catch (Exception ex)
            {
                // SOLID - S: Servis brinu se o greškama u podudaranju
                _logger.LogError($"Greška pri pronalaženju podudaranja: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// SOLID - S: Samo pretraživanje, bez dodatne logike.
        /// SOLID - O (Open/Closed): Lako je dodati nove filtere bez mijenjanja postojećeg koda.
        /// </summary>
        public async Task<IReadOnlyList<StudyPartner>> SearchPartnersAsync(
    string subject, string faculty, string level)
        {
            var all = await _partnerRepository.GetWhereAsync(x => true);
            var query = all.AsQueryable();

            if (!string.IsNullOrWhiteSpace(subject))
                query = query.Where(x => x.Subject.Contains(subject));

            if (!string.IsNullOrWhiteSpace(faculty))
                query = query.Where(x => x.Faculty.Contains(faculty));

            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(x => x.Level == level);

            return query.ToList();
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
