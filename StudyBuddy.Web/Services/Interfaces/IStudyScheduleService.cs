using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services.Interfaces;


/// <summary>
/// SOLID - ISP: Fokusira se samo na raspored, notifikacije su odjeljene u INotificationService.
/// SOLID - S: Servis ima jasnu, jednu odgovornost.
/// </summary>
public interface IStudyScheduleService
{
    Task<IReadOnlyList<StudySession>> GetSessionsForGroupAsync(int groupId);
    Task<StudySession> ScheduleAsync(int groupId, DateTime start, DateTime end, string location);
    Task CancelAsync(int sessionId);
    Task CompleteAsync(int sessionId);
}