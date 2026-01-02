using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services.Interfaces
{
    public interface IStudyGroupFacade
    {
        Task<IEnumerable<StudyGroup>> GetUserGroupsAsync(string userId);
        Task<IActionResult> CreateGroupAsync(CreateGroupViewModel model, string userId);
        Task<IActionResult> GetGroupDetailsAsync(int id);
        Task<GroupDetailsViewModel> GetMembersAsync(int id);
        Task<IActionResult> AddMemberAsync(int groupId, string userId, string role = "Member");
        Task<IActionResult> RemoveMemberAsync(int groupId, string userId);
    }

}
