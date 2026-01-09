using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudyBuddy.Web.Controllers;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.LearningGoalConfig;
using System.Security.Claims;
using Xunit;

namespace StudyBuddy.Test.Controllers
{
    public class LearningGoalsControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<ILearningGoalFacade> _mockFacade;
        private readonly LearningGoalsController _controller;
        private const string TestUserId = "test-user-123";

        public LearningGoalsControllerTests()
        {
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);
            _mockFacade = new Mock<ILearningGoalFacade>();
            _controller = new LearningGoalsController(_mockUserManager.Object, _mockFacade.Object);

            SetupUserClaims(TestUserId);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithGoals_WhenGoalsExist()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { Id = 1, Title = "Learn C#", Description = "Master C# basics", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(30) },
                new LearningGoal { Id = 2, Title = "Learn ASP.NET", Description = "Master ASP.NET Core", Progress = 30, TargetDate = DateTime.UtcNow.AddDays(60) }
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(TestUserId)).ReturnsAsync(goals);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<LearningGoal>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            _mockFacade.Verify(f => f.GetMyGoalsAsync(TestUserId), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsViewWithEmptyList_WhenNoGoals()
        {
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(TestUserId)).ReturnsAsync(new List<LearningGoal>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<LearningGoal>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_CallsFacadeWithCorrectUserId()
        {
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(It.IsAny<string>())).ReturnsAsync(new List<LearningGoal>());

            await _controller.Index();

            _mockFacade.Verify(f => f.GetMyGoalsAsync(TestUserId), Times.Once);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_Get_ReturnsViewWithDefaultModel()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearningGoal>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(LearningGoalConfig.Instance.DefaultProgress, model.Progress);
            Assert.NotEqual(default, model.TargetDate);
        }

        [Fact]
        public void Create_Get_SetsTargetDateFromConfig()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearningGoal>(viewResult.Model);
            var expectedDate = DateTime.Today.AddDays(LearningGoalConfig.Instance.DefaultDaysFromToday);
            Assert.Equal(expectedDate.Date, model.TargetDate.Date);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_CreatesGoal_WhenModelValid()
        {
            var model = new LearningGoal
            {
                Title = "Learn Docker",
                Description = "Master Docker containers",
                Progress = 0,
                TargetDate = DateTime.UtcNow.AddDays(45)
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.CreateMyGoalAsync(TestUserId, It.IsAny<LearningGoal>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(LearningGoalsController.Index), redirectResult.ActionName);
            _mockFacade.Verify(f => f.CreateMyGoalAsync(TestUserId, It.IsAny<LearningGoal>()), Times.Once);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewWithModel_WhenModelInvalid()
        {
            var model = new LearningGoal();
            _controller.ModelState.AddModelError("Title", "Title is required");

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            _mockFacade.Verify(f => f.CreateMyGoalAsync(It.IsAny<string>(), It.IsAny<LearningGoal>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_CallsFacadeWithCorrectParameters()
        {
            var model = new LearningGoal
            {
                Title = "Learn Kubernetes",
                Description = "Master K8s",
                Progress = 0,
                TargetDate = DateTime.UtcNow.AddDays(60)
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.CreateMyGoalAsync(It.IsAny<string>(), It.IsAny<LearningGoal>())).Returns(Task.CompletedTask);

            await _controller.Create(model);

            _mockFacade.Verify(f => f.CreateMyGoalAsync(TestUserId, It.Is<LearningGoal>(g =>
                g.Title == "Learn Kubernetes" &&
                g.Description == "Master K8s"
            )), Times.Once);
        }

        [Theory]
        [InlineData("Learn Python", "Master Python programming")]
        [InlineData("Learn React", "Master React framework")]
        [InlineData("Learn TypeScript", "Master TypeScript")]
        public async Task Create_Post_HandlesDifferentGoalTitles(string title, string description)
        {
            var model = new LearningGoal
            {
                Title = title,
                Description = description,
                Progress = 0,
                TargetDate = DateTime.UtcNow.AddDays(30)
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.CreateMyGoalAsync(It.IsAny<string>(), It.IsAny<LearningGoal>())).Returns(Task.CompletedTask);

            var result = await _controller.Create(model);

            Assert.IsType<RedirectToActionResult>(result);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_ReturnsViewWithGoal_WhenGoalExists()
        {
            var goal = new LearningGoal { Id = 1, Title = "Learn C#", Progress = 50, TargetDate = DateTime.UtcNow.AddDays(30) };
            var goals = new List<LearningGoal> { goal };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(TestUserId)).ReturnsAsync(goals);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearningGoal>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Learn C#", model.Title);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenGoalDoesNotExist()
        {
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(TestUserId)).ReturnsAsync(new List<LearningGoal>());

            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenGoalIdNotInUserGoals()
        {
            var goals = new List<LearningGoal>
            {
                new LearningGoal { Id = 1, Title = "Goal 1", Progress = 0, TargetDate = DateTime.UtcNow },
                new LearningGoal { Id = 2, Title = "Goal 2", Progress = 0, TargetDate = DateTime.UtcNow }
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(TestUserId)).ReturnsAsync(goals);

            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Edit_Get_WorksWithDifferentGoalIds(int goalId)
        {
            var goal = new LearningGoal { Id = goalId, Title = "Test Goal", Progress = 0, TargetDate = DateTime.UtcNow };
            var goals = new List<LearningGoal> { goal };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.GetMyGoalsAsync(TestUserId)).ReturnsAsync(goals);

            var result = await _controller.Edit(goalId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearningGoal>(viewResult.Model);
            Assert.Equal(goalId, model.Id);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_UpdatesGoal_WhenModelValid()
        {
            var model = new LearningGoal
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated description",
                Progress = 75,
                TargetDate = DateTime.UtcNow.AddDays(20)
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.UpdateMyGoalAsync(TestUserId, It.IsAny<LearningGoal>())).Returns(Task.CompletedTask);

            var result = await _controller.Edit(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(LearningGoalsController.Index), redirectResult.ActionName);
            _mockFacade.Verify(f => f.UpdateMyGoalAsync(TestUserId, It.IsAny<LearningGoal>()), Times.Once);
        }

        [Fact]
        public async Task Edit_Post_ReturnsViewWithModel_WhenModelInvalid()
        {
            var model = new LearningGoal { Id = 1 };
            _controller.ModelState.AddModelError("Title", "Title is required");

            var result = await _controller.Edit(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            _mockFacade.Verify(f => f.UpdateMyGoalAsync(It.IsAny<string>(), It.IsAny<LearningGoal>()), Times.Never);
        }

        [Fact]
        public async Task Edit_Post_UpdatesProgressField()
        {
            var model = new LearningGoal
            {
                Id = 1,
                Title = "Learn C#",
                Progress = 85,
                TargetDate = DateTime.UtcNow.AddDays(10)
            };
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.UpdateMyGoalAsync(It.IsAny<string>(), It.IsAny<LearningGoal>())).Returns(Task.CompletedTask);

            await _controller.Edit(model);

            _mockFacade.Verify(f => f.UpdateMyGoalAsync(TestUserId, It.Is<LearningGoal>(g =>
                g.Progress == 85
            )), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_Post_DeletesGoal_WhenValid()
        {
            int goalId = 1;
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.DeleteMyGoalAsync(TestUserId, goalId)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(goalId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(LearningGoalsController.Index), redirectResult.ActionName);
            _mockFacade.Verify(f => f.DeleteMyGoalAsync(TestUserId, goalId), Times.Once);
        }

        [Fact]
        public async Task Delete_Post_CallsFacadeWithCorrectParameters()
        {
            int goalId = 5;
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.DeleteMyGoalAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            await _controller.Delete(goalId);

            _mockFacade.Verify(f => f.DeleteMyGoalAsync(TestUserId, 5), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task Delete_Post_WorksWithDifferentGoalIds(int goalId)
        {
            _mockUserManager.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(TestUserId);
            _mockFacade.Setup(f => f.DeleteMyGoalAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);

            var result = await _controller.Delete(goalId);

            _mockFacade.Verify(f => f.DeleteMyGoalAsync(TestUserId, goalId), Times.Once);
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
