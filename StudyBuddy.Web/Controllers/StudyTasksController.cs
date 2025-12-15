using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudyBuddy.Web.Controllers
{
    [Authorize]
    public class StudyTasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudyTasksController(
            ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var weekStart = today.AddDays(-diff).Date;

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

            return View(model);
        }

        // GET: /StudyTasks/Create
        public IActionResult Create(string? date)
        {
            var model = new StudyTasks();

            if (!string.IsNullOrEmpty(date) &&
                DateTime.TryParse(date, out var parsedDate))
            {
                model.DueDate = parsedDate;
            }

            return View(model);
        }


        // POST: /StudyTasks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudyTasks studyTask)
        {
            if (!ModelState.IsValid)
            {
                return View(studyTask);
            }

            studyTask.UserId = _userManager.GetUserId(User);

            _context.StudyTasks.Add(studyTask);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Saved task for user {studyTask.UserId}, date {studyTask.DueDate}");
            return RedirectToAction(nameof(Index));


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

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




    }
}
