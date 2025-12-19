using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.ServicesImplementation
{
    /// <summary>
    /// SOLID - S: Samo upravlja rasporedima.
    /// SOLID - DIP: Notifikacije se ne pozivaju direktno, već kroz interfejs INotificationService.
    /// </summary>
    public class StudyScheduleService : IStudyScheduleService
    {
        private readonly IRepository<StudySession> _sessionRepository;
        private readonly IRepository<StudyGroup> _groupRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<StudyScheduleService> _logger;

        // SOLID - DIP: Konstruktor prima sve potrebne interfejse
        public StudyScheduleService(
            IRepository<StudySession> sessionRepository,
            IRepository<StudyGroup> groupRepository,
            INotificationService notificationService,
            ILogger<StudyScheduleService> logger)
        {
            _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
            _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<StudySession>> GetSessionsForGroupAsync(int groupId)
        {
            return await _sessionRepository.GetWhereAsync(x => x.GroupId == groupId);
        }

        /// <summary>
        /// SOLID - S: Samo kreira sesiju.
        /// SOLID - DIP: Notifikacije se delegira INotificationService, ne radi se direktno.
        /// </summary>
        public async Task<StudySession> ScheduleAsync(int groupId, DateTime start, DateTime end, string location)
        {
            // SOLID - S: Validacija je dio ove metode jer je odgovornost stvaranja
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException("Grupa nije pronađena.");

            if (start >= end)
                throw new ArgumentException("Početak mora biti prije kraja.");

            var session = new StudySession
            {
                GroupId = groupId,
                StartTime = start,
                EndTime = end,
                Location = location,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            // SOLID - DIP: Notifikacija se provodi kroz interfejs, ne direktno
            // Ako trebam promijeniti kako se prate obavijesti, ne moram mijenjati ovaj servis
            await _notificationService.NotifyGroupMembersAsync(groupId,
                $"Nova sesija zakazana: {start:dd.MM.yyyy HH:mm} na lokaciji {location}");

            _logger.LogInformation($"Sesija zakazana za grupu {groupId}.");
            return session;
        }

        public async Task CancelAsync(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                throw new KeyNotFoundException("Sesija nije pronađena.");

            session.Status = "Cancelled";
            await _sessionRepository.UpdateAsync(session);

            // SOLID - DIP: Ponovno koristi interfejs za obavijesti
            await _notificationService.NotifyGroupMembersAsync(session.GroupId,
                $"Sesija od {session.StartTime:dd.MM.yyyy HH:mm} je otkazana.");

            _logger.LogInformation($"Sesija {sessionId} otkazana.");
        }

        public async Task CompleteAsync(int sessionId)
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
                throw new KeyNotFoundException("Sesija nije pronađena.");

            session.Status = "Completed";
            await _sessionRepository.UpdateAsync(session);

            _logger.LogInformation($"Sesija {sessionId} označena kao završena.");
        }
    }

}
