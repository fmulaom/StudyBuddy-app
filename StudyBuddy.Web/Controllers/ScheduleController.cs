using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Controllers
{
    /// <summary>
    /// SOLID - S: Samo HTTP routing i delegiranje.
    /// SOLID - DIP: Koristi IStudyScheduleService i INotificationService interfejse.
    /// </summary>
    public class ScheduleController : Controller 
    {
        private readonly IStudyScheduleService _studyScheduleService;
        private readonly IStudyGroupService _groupService;

        public ScheduleController(
            IStudyScheduleService studyScheduleService,
            IStudyGroupService groupService
            )
        {
            _studyScheduleService = studyScheduleService ?? throw new ArgumentNullException(nameof(studyScheduleService));
            _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        }

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

        public IActionResult Create(int groupId)
        {
            return View(new ScheduleSessionViewModel { GroupId = groupId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ScheduleSessionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _studyScheduleService.ScheduleAsync(model.GroupId, model.StartTime, model.EndTime, model.Location); // Updated field name
                return RedirectToAction(nameof(Index), new { groupId = model.GroupId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int sessionId)
        {
            try
            {
                await _studyScheduleService.CancelAsync(sessionId);
                return Ok("Sesija otkazana.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
