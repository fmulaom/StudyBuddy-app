using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudyBuddy.Web.Controllers;

public class StudyGroupsController : Controller
{
    private readonly IStudyGroupFacade _facade;
    private readonly IStudyGroupService _groupService;
    private readonly ILogger<StudyGroupsController> _logger;

    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Member",
        "Admin",
        "Owner"
    };

    public StudyGroupsController(
        IStudyGroupFacade facade,
        IStudyGroupService groupService,
        ILogger<StudyGroupsController> logger)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogDebug("Listing study groups for user {UserToken}", ToUserToken(userId));

        var groups = await _facade.GetUserGroupsAsync(userId);
        return View(groups);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CreateGroupViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogInformation("Create group requested by user {UserToken}", ToUserToken(userId));

        return await _facade.CreateGroupAsync(model, userId);
    }

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
    public async Task<IActionResult> AddMember(int groupId, string userId, string role = "Member")
    {
        if (!AllowedRoles.Contains(role))
        {
            _logger.LogWarning("AddMember rejected for groupId {GroupId}: invalid role {Role}",
                groupId, LogSafe(role));
            return BadRequest("Invalid role.");
        }

        _logger.LogInformation("AddMember requested for groupId {GroupId}, targetUser {TargetUserToken}, role {Role}",
            groupId, ToUserToken(userId), role);

        return await _facade.AddMemberAsync(groupId, userId, role);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveMember(int groupId, string userId)
    {
        _logger.LogInformation("RemoveMember requested for groupId {GroupId}, targetUser {TargetUserToken}",
            groupId, ToUserToken(userId));

        return await _facade.RemoveMemberAsync(groupId, userId);
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
