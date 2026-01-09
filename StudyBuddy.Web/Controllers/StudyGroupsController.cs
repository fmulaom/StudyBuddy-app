using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using System.Security.Claims;

namespace StudyBuddy.Web.Controllers;
public class StudyGroupsController : Controller
{
    private readonly IStudyGroupFacade _facade;
    private readonly IStudyGroupService _groupService;

    public StudyGroupsController(IStudyGroupFacade facade, IStudyGroupService groupService)
    {
        _facade = facade ?? throw new ArgumentNullException(nameof(facade));
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var groups = await _facade.GetUserGroupsAsync(userId);
        return View(groups);
    }

    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(CreateGroupViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await _facade.CreateGroupAsync(model, userId);
    }

    public async Task<IActionResult> Details(int id)
    {
        var group = await _groupService.GetGroupByIdAsync(id);
        return group == null ? NotFound() : View(group);
    }

    public async Task<IActionResult> Members(int id)
    {
        var vm = await _facade.GetMembersAsync(id);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(int groupId, string userId, string role = "Member")
    {
        return await _facade.AddMemberAsync(groupId, userId, role);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveMember(int groupId, string userId)
    {
        return await _facade.RemoveMemberAsync(groupId, userId);
    }
}
