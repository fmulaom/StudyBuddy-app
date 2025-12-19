namespace StudyBuddy.Web.Services.Interfaces;
/// <summary>
/// SOLID - ISP: Odvojena od ostalih servisa.
/// SOLID - S: Samo šalje obavijesti.
/// Može se implementirati s email, SMS, push notifikacijama itd.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Pošalji obavijest članovima grupe.
    /// </summary>
    Task NotifyGroupMembersAsync(int groupId, string message);

    /// <summary>
    /// Pošalji podsjetnik o sesiji.
    /// </summary>
    Task SendSessionReminderAsync(int sessionId);
}