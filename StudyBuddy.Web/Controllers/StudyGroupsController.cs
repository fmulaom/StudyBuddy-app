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

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
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
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetUserId();
        var result = await _facade.CreateGroupAsync(model, userId);

        // Ako façade vraća IActionResult (npr. RedirectToAction ili View s error),
        // proslijedi direktno; ako vraća null, preusmjeri na Index
        return result ?? RedirectToAction(nameof(Index));
    }

    [HttpGet]
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int groupId, string userId, string role = "Member")
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, "Korisnik nije ispravan.");
            return RedirectToAction(nameof(Members), new { id = groupId });
        }

        var result = await _facade.AddMemberAsync(groupId, userId, role);
        return result ?? RedirectToAction(nameof(Members), new { id = groupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int groupId, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            ModelState.AddModelError(string.Empty, "Korisnik nije ispravan.");
            return RedirectToAction(nameof(Members), new { id = groupId });
        }

        var result = await _facade.RemoveMemberAsync(groupId, userId);
        return result ?? RedirectToAction(nameof(Members), new { id = groupId });
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Korisnik nije autentificiran.");
}
