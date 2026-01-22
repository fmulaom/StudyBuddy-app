using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using System.Security.Claims;

namespace StudyBuddy.Web.Controllers
{
    /// <summary>
    /// SOLID - S: Samo HTTP routing i delegiranje.
    /// SOLID - DIP: Koristi IStudyScheduleService i IStudyGroupService interfejse.
    /// </summary>
    public class ScheduleController : Controller
    {
        private readonly IStudyScheduleService _studyScheduleService;
        private readonly IStudyGroupService _groupService;

        public ScheduleController(
            IStudyScheduleService studyScheduleService,
            IStudyGroupService groupService)
        {
            _studyScheduleService = studyScheduleService ?? throw new ArgumentNullException(nameof(studyScheduleService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(int groupId)
        {
            var group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
                return NotFound();

            var sessions = await _studyScheduleService.GetSessionsForGroupAsync(groupId);

            var vm = new GroupDetailsViewModel
            {
                Group = group,
                Sessions = sessions
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create(int groupId)
        {
            return View(new ScheduleSessionViewModel { GroupId = groupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleSessionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _studyScheduleService.ScheduleAsync(
                    model.GroupId,
                    model.StartTime,
                    model.EndTime,
                    model.Location);

                return RedirectToAction(nameof(Index), new { groupId = model.GroupId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Greška pri kreiranju sesije: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int sessionId, int groupId)
        {
            try
            {
                await _studyScheduleService.CancelAsync(sessionId);
                return RedirectToAction(nameof(Index), new { groupId = groupId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Greška pri otkazivanju sesije: {ex.Message}");
            }
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
    }
}
