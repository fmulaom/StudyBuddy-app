using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace StudyBuddy.Web.Services.StudyGroupConfig
{
    public class StudyGroupFacade : IStudyGroupFacade
    {
        private readonly IStudyGroupService _groupService;
        private readonly IRepository<StudyGroupMember> _memberRepo;
        private readonly IResourceService _resourceService;
        private readonly ILogger<StudyGroupFacade> _logger;

        private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Member",
            "Admin",
            "Owner"
        };

        public StudyGroupFacade(
            IStudyGroupService groupService,
            IRepository<StudyGroupMember> memberRepo,
            IResourceService resourceService,
            ILogger<StudyGroupFacade> logger)
        {
            _groupService = groupService;
            _memberRepo = memberRepo;
            _resourceService = resourceService;
            _logger = logger;
        }

        public async Task<IEnumerable<StudyGroup>> GetUserGroupsAsync(string userId)
        {
            return await _groupService.GetUserGroupsAsync(userId);
        }

        public async Task<IActionResult> CreateGroupAsync(CreateGroupViewModel model, string userId)
        {
            if (model == null || string.IsNullOrEmpty(model.Name))
                return new BadRequestResult();

            try
            {
                var group = await _groupService.CreateGroupAsync(userId, model.Name);

                _logger.LogInformation("Group created. GroupId {GroupId}, GroupName {GroupName}, CreatedBy {UserToken}",
                    group.Id,
                    LogSafe(group.Name),
                    ToUserToken(userId));

                return new RedirectToActionResult("Index", "StudyGroups", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create group. User {UserToken}", ToUserToken(userId));
                return new BadRequestObjectResult("Error creating group.");
            }
        }

        public async Task<GroupDetailsViewModel> GetMembersAsync(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
                throw new KeyNotFoundException("Group not found");

            var members = await _memberRepo.GetWhereAsync(x => x.GroupId == id);
            var resources = await _resourceService.GetGroupResourcesAsync(id);

            return new GroupDetailsViewModel
            {
                Group = group,
                Members = members,
                Resources = resources
            };
        }

        public async Task<IActionResult> AddMemberAsync(int groupId, string userId, string role = "Member")
        {
            if (!AllowedRoles.Contains(role))
            {
                _logger.LogWarning("AddMember rejected. GroupId {GroupId}, Role {Role}, TargetUser {UserToken}",
                    groupId, LogSafe(role), ToUserToken(userId));
                return new BadRequestObjectResult("Invalid role.");
            }

            try
            {
                await _groupService.AddMemberAsync(groupId, userId, role);

                _logger.LogInformation("Member added. GroupId {GroupId}, Role {Role}, TargetUser {UserToken}",
                    groupId, role, ToUserToken(userId));

                return new RedirectToActionResult("Members", "StudyGroups", new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add member. GroupId {GroupId}, TargetUser {UserToken}",
                    groupId, ToUserToken(userId));
                return new BadRequestObjectResult("Error adding member.");
            }
        }

        public async Task<IActionResult> RemoveMemberAsync(int groupId, string userId)
        {
            try
            {
                await _groupService.RemoveMemberAsync(groupId, userId);

                _logger.LogInformation("Member removed. GroupId {GroupId}, TargetUser {UserToken}",
                    groupId, ToUserToken(userId));

                return new RedirectToActionResult("Members", "StudyGroups", new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove member. GroupId {GroupId}, TargetUser {UserToken}",
                    groupId, ToUserToken(userId));
                return new BadRequestObjectResult("Error removing member.");
            }
        }

        public async Task<IActionResult> GetGroupDetailsAsync(int id)
        {
            var g = await _groupService.GetGroupByIdAsync(id);
            return g == null ? new NotFoundResult() : new OkObjectResult(g);
        }

        private static string LogSafe(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (!char.IsControl(ch)) sb.Append(ch);
            }

            return sb.Length <= 64 ? sb.ToString() : sb.ToString(0, 64);
        }

        private static string ToUserToken(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return "anonymous";

            var cleaned = LogSafe(userId);
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(cleaned));
            return Convert.ToHexString(hash)[..12];
        }
    }
}
