using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.ServicesImplementation
{
    /// <summary>
    /// SOLID - S: Samo upravlja obavijestima. Ako trebam email/SMS, pravim novu implementaciju.
    /// SOLID - O: Otvorena za proširenje (email, SMS, push) bez mijenjanja postojećeg koda.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Zasad - konzola obavijest. Kasnije se može zamijeniti s email/SMS.
        /// </summary>
        public Task NotifyGroupMembersAsync(int groupId, string message)
        {
            // SOLID - S: Samo slanje obavijesti
            _logger.LogInformation($"[NOTIFIKACIJA - GRUPA {groupId}] {message}");

            // Kasnije: await _emailService.SendAsync(...);
            // ili: await _smsService.SendAsync(...);

            return Task.CompletedTask;
        }

        public Task SendSessionReminderAsync(int sessionId)
        {
            _logger.LogInformation($"[PODSJETNIK - SESIJA {sessionId}] Vaša sesija počinje za 15 minuta!");
            return Task.CompletedTask;
        }
    }
}
