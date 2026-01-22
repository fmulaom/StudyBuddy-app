using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Constants;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.Logging;
using System.Security.Claims;

namespace StudyBuddy.Web.Controllers;

public class StudyGroupsController : Controller
{
    private readonly IStudyGroupFacade _facade;
    private readonly IStudyGroupService _groupService;
    private readonly ILogger<StudyGroupsController> _logger;

    public StudyGroupsController(
        IStudyGroupFacade facade,
        IStudyGroupService groupService,
        ILogger<StudyGroupsController> logger)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        _logger.LogDebug("Listing study groups for user {UserToken}", LogValue.UserToken(userId));

        var groups = await _facade.GetUserGroupsAsync(userId);
        return View(groups);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        _logger.LogInformation("Create group requested by user {UserToken}", LogValue.UserToken(userId));

        return await _facade.CreateGroupAsync(model, userId);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        _logger.LogDebug("Group details requested for groupId {GroupId}", id);

        var group = await _groupService.GetGroupByIdAsync(id);
        return group == null ? NotFound() : View(group);
    }

    public async Task<IActionResult> Members(int id)
    {
        _logger.LogDebug("Members requested for groupId {GroupId}", id);

        var vm = await _facade.GetMembersAsync(id);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(int groupId, string userId, string role = StudyGroupRoles.Member)
    {
        if (!StudyGroupRoles.IsAllowed(role))
        {
            _logger.LogWarning("AddMember rejected for groupId {GroupId}: invalid role {Role}",
                groupId, LogValue.Safe(role));
            return BadRequest("Invalid role.");
        }

        _logger.LogInformation("AddMember requested for groupId {GroupId}, targetUser {TargetUserToken}, role {Role}",
            groupId, LogValue.UserToken(userId), role);

        return await _facade.AddMemberAsync(groupId, userId, role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int groupId, string userId)
    {
        _logger.LogInformation("RemoveMember requested for groupId {GroupId}, targetUser {TargetUserToken}",
            groupId, LogValue.UserToken(userId));

        return await _facade.RemoveMemberAsync(groupId, userId);
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Korisnik nije autentificiran.");
}
