using Microsoft.AspNetCore.Mvc;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using System.Security.Claims;

namespace StudyBuddy.Web.Controllers
{
    public class PartnerMatchingController : Controller
    {
        private readonly IPartnerMatchingService _matchingService;
        private readonly IRepository<StudyPartner> _partnerRepository;

        public PartnerMatchingController(
            IPartnerMatchingService matchingService,
            IRepository<StudyPartner> partnerRepository)
        {
            _matchingService = matchingService ?? throw new ArgumentNullException(nameof(matchingService));
            _partnerRepository = partnerRepository ?? throw new ArgumentNullException(nameof(partnerRepository));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var matches = await _matchingService.GetAutomaticMatchesAsync(userId);
            var vm = new PartnerSearchViewModel { Results = matches.ToList() };
            return View(vm);
        }

        [HttpGet]
        public IActionResult Search()
        {
            var vm = new PartnerSearchViewModel
            {
                Results = new List<StudyPartner>()
            };
            ViewBag.Searched = false;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(PartnerSearchViewModel model)
        {
            var subject = model.Subject ?? string.Empty;
            var faculty = model.Faculty ?? string.Empty;
            var level = model.Level ?? string.Empty;

            var results = await _matchingService.SearchPartnersAsync(subject, faculty, level);

            model.Results = results.ToList();
            ViewBag.Searched = true;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFavorite(int partnerId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            await _matchingService.MarkFavoriteAsync(userId, partnerId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var existing = (await _partnerRepository
                .GetWhereAsync(x => x.UserId == userId))
                .FirstOrDefault();

            var vm = new EditStudyProfileViewModel
            {
                Subject = existing?.Subject ?? string.Empty,
                Faculty = existing?.Faculty ?? string.Empty,
                Level = existing?.Level ?? string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditStudyProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var existingList = await _partnerRepository.GetWhereAsync(x => x.UserId == userId);
            var existing = existingList.FirstOrDefault();

            if (existing == null)
            {
                var partner = new StudyPartner
                {
                    UserId = userId,
                    Subject = model.Subject,
                    Faculty = model.Faculty,
                    Level = model.Level,
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _partnerRepository.AddAsync(partner);
            }
            else
            {
                existing.Subject = model.Subject;
                existing.Faculty = model.Faculty;
                existing.Level = model.Level;

                await _partnerRepository.UpdateAsync(existing);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
