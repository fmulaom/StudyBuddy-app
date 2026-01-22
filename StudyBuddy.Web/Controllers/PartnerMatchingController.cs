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
            _matchingService = matchingService;
            _partnerRepository = partnerRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var matches = await _matchingService.GetAutomaticMatchesAsync(userId);
            return View(new PartnerSearchViewModel { Results = matches.ToList() });
        }

        [HttpGet]
        public IActionResult Search()
        {
            ViewBag.Searched = false;
            return View(new PartnerSearchViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(PartnerSearchViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Searched = false;
                return View(model);
            }

            var results = await _matchingService.SearchPartnersAsync(
                model.Subject ?? "",
                model.Faculty ?? "",
                model.Level ?? "");

            model.Results = results.ToList();
            ViewBag.Searched = true;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFavorite(int partnerId)
        {
            var userId = GetUserId();
            await _matchingService.MarkFavoriteAsync(userId, partnerId);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = GetUserId();
            var existing = (await _partnerRepository.GetWhereAsync(x => x.UserId == userId)).FirstOrDefault();

            return View(new EditStudyProfileViewModel
            {
                Subject = existing?.Subject ?? "",
                Faculty = existing?.Faculty ?? "",
                Level = existing?.Level ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditStudyProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserId();
            await UpsertPartnerProfileAsync(userId, model);
            return RedirectToAction(nameof(Index));
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();

        private async Task UpsertPartnerProfileAsync(string userId, EditStudyProfileViewModel model)
        {
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
        }
    }
}
