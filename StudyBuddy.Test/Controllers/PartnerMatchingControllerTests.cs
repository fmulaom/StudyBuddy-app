using Microsoft.AspNetCore.Http;
// ...existing code...
// (No changes needed, file already contains comprehensive unit tests.)
// ...existing code...
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyBuddy.Web.Controllers;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using System.Linq.Expressions;
using System.Security.Claims;
using Xunit;

namespace StudyBuddy.Test.Controllers
{
    public class PartnerMatchingControllerTests
    {
        private readonly Mock<IPartnerMatchingService> _mockMatchingService;
        private readonly Mock<IRepository<StudyPartner>> _mockRepository;
        private readonly PartnerMatchingController _controller;
        private const string TestUserId = "test-user-123";

        public PartnerMatchingControllerTests()
        {
            _mockMatchingService = new Mock<IPartnerMatchingService>();
            _mockRepository = new Mock<IRepository<StudyPartner>>();
            _controller = new PartnerMatchingController(_mockMatchingService.Object, _mockRepository.Object);

            SetupUserClaims(TestUserId);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithMatches_WhenMatchesExist()
        {
            var matches = new List<StudyPartner>
            {
                new StudyPartner { Id = 1, UserId = "user1", Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow },
                new StudyPartner { Id = 2, UserId = "user2", Subject = "Physics", Faculty = "Science", Level = "Intermediate", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };
            _mockMatchingService.Setup(s => s.GetAutomaticMatchesAsync(TestUserId)).ReturnsAsync(matches);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PartnerSearchViewModel>(viewResult.Model);
            Assert.Equal(2, model.Results.Count);
            _mockMatchingService.Verify(s => s.GetAutomaticMatchesAsync(TestUserId), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsViewWithEmptyList_WhenNoMatches()
        {
            _mockMatchingService.Setup(s => s.GetAutomaticMatchesAsync(TestUserId)).ReturnsAsync(new List<StudyPartner>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PartnerSearchViewModel>(viewResult.Model);
            Assert.Empty(model.Results);
        }

        [Fact]
        public async Task Index_CallsServiceWithCorrectUserId()
        {
            _mockMatchingService.Setup(s => s.GetAutomaticMatchesAsync(It.IsAny<string>())).ReturnsAsync(new List<StudyPartner>());

            await _controller.Index();

            _mockMatchingService.Verify(s => s.GetAutomaticMatchesAsync(TestUserId), Times.Once);
        }

        #endregion

        #region Search GET Tests

        [Fact]
        public void Search_Get_ReturnsView_WithEmptyModel()
        {
            var result = _controller.Search();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PartnerSearchViewModel>(viewResult.Model);
            Assert.Empty(model.Results);
            Assert.False((bool)_controller.ViewBag.Searched);
        }

        #endregion

        #region Search POST Tests

        [Fact]
        public async Task Search_Post_ReturnsResults_WhenPartnerExists()
        {
            var model = new PartnerSearchViewModel
            {
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced"
            };
            var partners = new List<StudyPartner>
            {
                new StudyPartner { Id = 1, Subject = "Math", Faculty = "Science", Level = "Advanced", IsFavorite = false, CreatedAt = DateTime.UtcNow }
            };
            _mockMatchingService.Setup(s => s.SearchPartnersAsync("Math", "Science", "Advanced")).ReturnsAsync(partners);

            var result = await _controller.Search(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<PartnerSearchViewModel>(viewResult.Model);
            Assert.Single(returnedModel.Results);
            Assert.True((bool)_controller.ViewBag.Searched);
            _mockMatchingService.Verify(s => s.SearchPartnersAsync("Math", "Science", "Advanced"), Times.Once);
        }

        [Fact]
        public async Task Search_Post_ReturnsEmptyResults_WhenNoPartnersFound()
        {
            var model = new PartnerSearchViewModel
            {
                Subject = "Unknown",
                Faculty = "Unknown",
                Level = "Unknown"
            };
            _mockMatchingService.Setup(s => s.SearchPartnersAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<StudyPartner>());

            var result = await _controller.Search(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<PartnerSearchViewModel>(viewResult.Model);
            Assert.Empty(returnedModel.Results);
            Assert.True((bool)_controller.ViewBag.Searched);
        }

        [Fact]
        public async Task Search_Post_HandlesNullValues_ByConvertingToEmptyString()
        {
            var model = new PartnerSearchViewModel { Subject = null, Faculty = null, Level = null };
            _mockMatchingService.Setup(s => s.SearchPartnersAsync("", "", "")).ReturnsAsync(new List<StudyPartner>());

            await _controller.Search(model);

            _mockMatchingService.Verify(s => s.SearchPartnersAsync("", "", ""), Times.Once);
        }

        [Theory]
        [InlineData("Math", "Science", "Advanced")]
        [InlineData("Physics", "Science", "Intermediate")]
        [InlineData("Biology", "Medicine", "Beginner")]
        public async Task Search_Post_PassesCorrectParametersToService(string subject, string faculty, string level)
        {
            var model = new PartnerSearchViewModel { Subject = subject, Faculty = faculty, Level = level };
            _mockMatchingService.Setup(s => s.SearchPartnersAsync(subject, faculty, level)).ReturnsAsync(new List<StudyPartner>());

            await _controller.Search(model);

            _mockMatchingService.Verify(s => s.SearchPartnersAsync(subject, faculty, level), Times.Once);
        }

        #endregion

        #region MarkFavorite Tests

        [Fact]
        public async Task MarkFavorite_CallsServiceWithCorrectParameters()
        {
            int partnerId = 42;
            _mockMatchingService.Setup(s => s.MarkFavoriteAsync(TestUserId, partnerId)).Returns(Task.CompletedTask);

            var result = await _controller.MarkFavorite(partnerId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PartnerMatchingController.Index), redirectResult.ActionName);
            _mockMatchingService.Verify(s => s.MarkFavoriteAsync(TestUserId, partnerId), Times.Once);
        }

        [Fact]
        public async Task MarkFavorite_RedirectsToIndex()
        {
            _mockMatchingService.Setup(s => s.MarkFavoriteAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.MarkFavorite(1);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PartnerMatchingController.Index), redirectResult.ActionName);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(999)]
        public async Task MarkFavorite_WorksWithDifferentPartnerIds(int partnerId)
        {
            _mockMatchingService.Setup(s => s.MarkFavoriteAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.MarkFavorite(partnerId);

            _mockMatchingService.Verify(s => s.MarkFavoriteAsync(TestUserId, partnerId), Times.Once);
        }

        #endregion

        #region EditProfile GET Tests

        [Fact]
        public async Task EditProfile_Get_ReturnsViewWithExistingProfile()
        {
            var existingPartner = new StudyPartner
            {
                Id = 1,
                UserId = TestUserId,
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            _mockRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyPartner, bool>>>()))
                .ReturnsAsync(new List<StudyPartner> { existingPartner });

            var result = await _controller.EditProfile();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditStudyProfileViewModel>(viewResult.Model);
            Assert.Equal("Math", model.Subject);
            Assert.Equal("Science", model.Faculty);
            Assert.Equal("Advanced", model.Level);
        }

        [Fact]
        public async Task EditProfile_Get_ReturnsViewWithEmptyModel_WhenNoExistingProfile()
        {
            _mockRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyPartner, bool>>>()))
                .ReturnsAsync(new List<StudyPartner>());

            var result = await _controller.EditProfile();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditStudyProfileViewModel>(viewResult.Model);
            Assert.Equal("", model.Subject);
            Assert.Equal("", model.Faculty);
            Assert.Equal("", model.Level);
        }

        #endregion

        #region EditProfile POST Tests

        [Fact]
        public async Task EditProfile_Post_CreatesNewProfile_WhenNotExists()
        {
            var model = new EditStudyProfileViewModel
            {
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced"
            };
            _mockRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyPartner, bool>>>()))
                .ReturnsAsync(new List<StudyPartner>());
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<StudyPartner>())).Returns(Task.CompletedTask);

            var result = await _controller.EditProfile(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PartnerMatchingController.Index), redirectResult.ActionName);
            _mockRepository.Verify(r => r.AddAsync(It.Is<StudyPartner>(p =>
                p.UserId == TestUserId &&
                p.Subject == "Math" &&
                p.Faculty == "Science" &&
                p.Level == "Advanced" &&
                !p.IsFavorite &&
                p.CreatedAt != default
            )), Times.Once);
        }

        [Fact]
        public async Task EditProfile_Post_UpdatesExistingProfile_WhenExists()
        {
            var existingPartner = new StudyPartner
            {
                Id = 1,
                UserId = TestUserId,
                Subject = "Physics",
                Faculty = "Science",
                Level = "Intermediate",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };
            var model = new EditStudyProfileViewModel
            {
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced"
            };
            _mockRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyPartner, bool>>>()))
                .ReturnsAsync(new List<StudyPartner> { existingPartner });
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<StudyPartner>())).Returns(Task.CompletedTask);

            var result = await _controller.EditProfile(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(PartnerMatchingController.Index), redirectResult.ActionName);
            _mockRepository.Verify(r => r.UpdateAsync(It.Is<StudyPartner>(p =>
                p.Id == 1 &&
                p.Subject == "Math" &&
                p.Faculty == "Science" &&
                p.Level == "Advanced"
            )), Times.Once);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<StudyPartner>()), Times.Never);
        }

        [Fact]
        public async Task EditProfile_Post_ReturnsView_WhenModelStateInvalid()
        {
            var model = new EditStudyProfileViewModel();
            _controller.ModelState.AddModelError("Subject", "Required");

            var result = await _controller.EditProfile(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<StudyPartner>()), Times.Never);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<StudyPartner>()), Times.Never);
        }

        #endregion

        #region Helper Methods

        private void SetupUserClaims(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(claimsIdentity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        #endregion
    }
}
