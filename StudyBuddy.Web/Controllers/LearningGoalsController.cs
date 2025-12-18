using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyBuddy.Web.Data;
using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Controllers
{
    [Authorize]
    // S - Single Responsibility:
   
    public class LearningGoalsController : Controller
    {
        // D - Dependency Inversion:
      
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LearningGoalsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /LearningGoals
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var goals = await _context.LearningGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();

            // S - Controller samo dohvaća podatke i šalje ih u view,
            
            return View(goals);
        }

        // GET: /LearningGoals/Create
        public IActionResult Create()
        {
            var model = new LearningGoal
            {
                TargetDate = DateTime.Today.AddDays(7),
                Progress = 0
            };

            return View(model);
        }

        // POST: /LearningGoals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LearningGoal learningGoal)
        {
            if (!ModelState.IsValid)
            {
                return View(learningGoal);
            }

            learningGoal.UserId = _userManager.GetUserId(User);

            _context.LearningGoals.Add(learningGoal);
            await _context.SaveChangesAsync();

           
            return RedirectToAction(nameof(Index));
        }

        // GET: /LearningGoals/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            var goal = await _context.LearningGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // POST: /LearningGoals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LearningGoal model)
        {
            var userId = _userManager.GetUserId(User);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var goal = await _context.LearningGoals
                .FirstOrDefaultAsync(g => g.Id == model.Id && g.UserId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            goal.Title = model.Title;
            goal.Description = model.Description;
            goal.TargetDate = model.TargetDate;
            goal.Progress = model.Progress;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /LearningGoals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            var goal = await _context.LearningGoals
                .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

            if (goal == null)
            {
                return NotFound();
            }

            _context.LearningGoals.Remove(goal);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
