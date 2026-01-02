using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.StudyGroupConfig
{
    public class StudyGroupFacade : IStudyGroupFacade
    {
        private readonly IStudyGroupService _groupService;
        private readonly IRepository<StudyGroupMember> _memberRepo;
        private readonly IResourceService _resourceService;
        private readonly ILogger<StudyGroupFacade> _logger;

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
                _logger.LogInformation("Group {GroupName} created by {UserId}", group.Name, userId);
                return new RedirectToActionResult("Index", "StudyGroups", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create group for {UserId}", userId);
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
            try
            {
                await _groupService.AddMemberAsync(groupId, userId, role);
                return new RedirectToActionResult("Members", "StudyGroups", new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add member {UserId} to group {GroupId}", userId, groupId);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<IActionResult> RemoveMemberAsync(int groupId, string userId)
        {
            try
            {
                await _groupService.RemoveMemberAsync(groupId, userId);
                return new RedirectToActionResult("Members", "StudyGroups", new { id = groupId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove member {UserId} from group {GroupId}", userId, groupId);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public async Task<IActionResult> GetGroupDetailsAsync(int id)
        {
            var g = await _groupService.GetGroupByIdAsync(id);
            return g == null ? new NotFoundResult() : new OkObjectResult(g);
        }
    }

}
