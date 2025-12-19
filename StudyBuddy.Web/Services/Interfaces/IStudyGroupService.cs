using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services.Interfaces;


/// <summary>
/// SOLID - ISP: Interfejs je odvojen od notifikacija i raspored - svaki servis ima svoju odgovornost.
/// SOLID - DIP: Apstrakcija omogućava fleksibilnu zamjenu implementacija.
/// </summary>
public interface IStudyGroupService
{
    Task<IReadOnlyList<StudyGroup>> GetUserGroupsAsync(string userId);
    Task<StudyGroup> GetGroupByIdAsync(int groupId);
    Task<StudyGroup> CreateGroupAsync(string ownerId, string name);
    Task AddMemberAsync(int groupId, string userId, string role = "Member");
    Task RemoveMemberAsync(int groupId, string userId);
    Task DeleteGroupAsync(int groupId);
}