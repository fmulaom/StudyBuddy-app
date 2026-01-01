using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.LearningGoalConfig;

namespace StudyBuddy.Web.Controllers
{
    [Authorize]
    public class LearningGoalsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILearningGoalFacade _facade;
        private readonly LearningGoalConfig _config = LearningGoalConfig.Instance; // SINGLETON

        public LearningGoalsController(
            UserManager<ApplicationUser> userManager,
            ILearningGoalFacade facade)
        {
            _userManager = userManager;
            _facade = facade;
        }

        // GET: /LearningGoals
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            // FACADE – umjesto direktnog _context
            var goals = await _facade.GetMyGoalsAsync(userId);

            return View(goals);
        }

        // GET: /LearningGoals/Create
        public IActionResult Create()
        {
            // SINGLETON – konfiguracija default vrijednosti
            var model = new LearningGoal
            {
                TargetDate = DateTime.Today.AddDays(_config.DefaultDaysFromToday),
                Progress = _config.DefaultProgress
            };

            return View(model);
        }

        // POST: /LearningGoals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LearningGoal model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            // FACADE – unutar sebe poziva ILearningGoalService
            await _facade.CreateMyGoalAsync(userId, model);

            return RedirectToAction(nameof(Index));
        }

        // GET: /LearningGoals/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Preko FACADE dohvaćamo ciljeve pa filtriramo po id
            var goals = await _facade.GetMyGoalsAsync(userId);
            var goal = goals.FirstOrDefault(g => g.Id == id);

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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            // FACADE → ILearningGoalService → STRATEGY za progress
            await _facade.UpdateMyGoalAsync(userId, model);

            return RedirectToAction(nameof(Index));
        }

        // POST: /LearningGoals/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);

            // FACADE – brisanje cilja za korisnika
            await _facade.DeleteMyGoalAsync(userId, id);

            return RedirectToAction(nameof(Index));
        }
    }
}
