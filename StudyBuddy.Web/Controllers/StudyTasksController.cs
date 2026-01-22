using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using System.Globalization;
using System.Security.Claims;

namespace StudyBuddy.Web.Controllers
{
    [Authorize]
    public class StudyTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudyTasksController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        public async Task<IActionResult> Index(int weekOffset = 0)
        {
            var userId = GetUserId();

            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var baseWeekStart = today.AddDays(-diff).Date;
            var weekStart = baseWeekStart.AddDays(weekOffset * 7);

            var tasks = await _context.StudyTasks
                .Where(t =>
                    t.UserId == userId &&
                    t.DueDate.Date >= weekStart &&
                    t.DueDate.Date < weekStart.AddDays(7))
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var model = new WeeklyCalendarViewModel
            {
                WeekStart = weekStart,
                TasksByDay = tasks
                    .GroupBy(t => t.DueDate.DayOfWeek)
                    .ToDictionary(g => g.Key, g => g.ToList())
            };

            ViewBag.WeekOffset = weekOffset;
            return View(model);
        }

        [HttpGet]
        public IActionResult Create(string? date)
        {
            var model = new StudyTasks();

            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDate))
            {
                model.DueDate = parsedDate;
            }
            else
            {
                model.DueDate = DateTime.Today;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudyTasks studyTask)
        {
            if (!ModelState.IsValid)
            {
                return View(studyTask);
            }

            studyTask.UserId = GetUserId();

            _context.StudyTasks.Add(studyTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            var task = await _context.StudyTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            _context.StudyTasks.Remove(task);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();

            var task = await _context.StudyTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudyTasks studyTask)
        {
            if (id != studyTask.Id)
            {
                return BadRequest("ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return View(studyTask);
            }

            var userId = GetUserId();
            var existingTask = await _context.StudyTasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = studyTask.Title;
            existingTask.Description = studyTask.Description;
            existingTask.DueDate = studyTask.DueDate;

            _context.StudyTasks.Update(existingTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Korisnik nije autentificiran.");
    }
}
