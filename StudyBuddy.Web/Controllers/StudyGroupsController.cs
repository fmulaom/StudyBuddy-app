using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using System.Security.Claims;

namespace StudyBuddy.Web.Controllers
{
    /// <summary>
    /// SOLID - S: Samo HTTP routing. Sva logika je u IStudyGroupService.
    /// SOLID - DIP: Ovisi o interfejsu, ne o klasi.
    /// </summary>
    public class StudyGroupsController : Controller
    {
        private readonly IStudyGroupService _groupService;
        private readonly IRepository<StudyGroupMember> _memberRepository;
        private readonly IResourceService _resourceService;

        public StudyGroupsController(
            IStudyGroupService groupService,
            IRepository<StudyGroupMember> memberRepository,
            IResourceService resourceService,
            ILogger<StudyGroupsController> logger)
        {
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var groups = await _groupService.GetUserGroupsAsync(userId);
            return View(groups);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGroupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var group = await _groupService.CreateGroupAsync(userId, model.Name);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest("Greška pri kreiranju grupe.");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound();
            return View(group);
        }

        public async Task<IActionResult> Members(int id)
        {
            var group = await _groupService.GetGroupByIdAsync(id);
            if (group == null)
                return NotFound();

            var members = await _memberRepository.GetWhereAsync(x => x.GroupId == id);
            var resources = await _resourceService.GetGroupResourcesAsync(id);

            var vm = new GroupDetailsViewModel
            {
                Group = group,
                Members = members,
                Resources = resources
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(int groupId, string userId, string role = "Member")
        {
            try
            {
                await _groupService.AddMemberAsync(groupId, userId, role);
                return RedirectToAction(nameof(Members), new { id = groupId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveMember(int groupId, string userId)
        {
            try
            {
                await _groupService.RemoveMemberAsync(groupId, userId);
                return RedirectToAction(nameof(Members), new { id = groupId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
