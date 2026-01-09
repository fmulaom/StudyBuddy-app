using Moq;
using StudyBuddy.Web.Controllers;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace StudyBuddy.Test.Controllers
{
    public class ScheduleControllerTests
    {
        private readonly Mock<IStudyScheduleService> _mockScheduleService;
        private readonly Mock<IStudyGroupService> _mockGroupService;
        private readonly ScheduleController _controller;

        public ScheduleControllerTests()
        {
            _mockScheduleService = new Mock<IStudyScheduleService>();
            _mockGroupService = new Mock<IStudyGroupService>();
            _controller = new ScheduleController(_mockScheduleService.Object, _mockGroupService.Object);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithGroupDetailsViewModel_WhenGroupExists()
        {
            var groupId = 1;
            var group = new StudyGroup
            {
                Id = groupId,
                Name = "Math Group",
                OwnerId = "user1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var sessions = new List<StudySession>
            {
                new StudySession
                {
                    Id = 1,
                    GroupId = groupId,
                    StartTime = DateTime.UtcNow.AddDays(1),
                    EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                    Location = "Room 101",
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId))
                .ReturnsAsync(group);
            _mockScheduleService.Setup(s => s.GetSessionsForGroupAsync(groupId))
                .ReturnsAsync(sessions);

            var result = await _controller.Index(groupId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupDetailsViewModel>(viewResult.Model);
            Assert.Equal(group.Id, model.Group.Id);
            Assert.Single(model.Sessions);
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var groupId = 999;
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId))
                .ReturnsAsync((StudyGroup)null);

            var result = await _controller.Index(groupId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Index_CallsServiceWithCorrectGroupId()
        {
            var groupId = 5;
            var group = new StudyGroup
            {
                Id = groupId,
                Name = "Group",
                OwnerId = "user1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId))
                .ReturnsAsync(group);
            _mockScheduleService.Setup(s => s.GetSessionsForGroupAsync(groupId))
                .ReturnsAsync(new List<StudySession>());

            await _controller.Index(groupId);

            _mockGroupService.Verify(s => s.GetGroupByIdAsync(groupId), Times.Once);
            _mockScheduleService.Verify(s => s.GetSessionsForGroupAsync(groupId), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsEmptySessionsList_WhenNoSessions()
        {
            var groupId = 1;
            var group = new StudyGroup
            {
                Id = groupId,
                Name = "Group",
                OwnerId = "user1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId))
                .ReturnsAsync(group);
            _mockScheduleService.Setup(s => s.GetSessionsForGroupAsync(groupId))
                .ReturnsAsync(new List<StudySession>());

            var result = await _controller.Index(groupId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupDetailsViewModel>(viewResult.Model);
            Assert.Empty(model.Sessions);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_GET_ReturnsViewWithScheduleSessionViewModel()
        {
            var result = _controller.Create(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ScheduleSessionViewModel>(viewResult.Model);
            Assert.Equal(1, model.GroupId);
        }

        [Fact]
        public void Create_GET_SetsGroupIdFromRoute()
        {
            int groupId = 42;

            var result = _controller.Create(groupId) as ViewResult;
            var model = result.Model as ScheduleSessionViewModel;

            Assert.NotNull(model);
            Assert.Equal(groupId, model.GroupId);
        }

        #endregion

        #region Cancel Tests

        [Fact]
        public async Task Cancel_ReturnsOkResult_WhenSessionCanceled()
        {
            var sessionId = 1;
            _mockScheduleService.Setup(s => s.CancelAsync(sessionId))
                .Returns(Task.CompletedTask);

            var result = await _controller.Cancel(sessionId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Sesija otkazana.", okResult.Value);
        }

        [Fact]
        public async Task Cancel_CallsServiceWithCorrectSessionId()
        {
            var sessionId = 42;
            _mockScheduleService.Setup(s => s.CancelAsync(sessionId))
                .Returns(Task.CompletedTask);

            await _controller.Cancel(sessionId);

            _mockScheduleService.Verify(s => s.CancelAsync(sessionId), Times.Once);
        }

        [Fact]
        public async Task Cancel_ReturnsBadRequest_WhenExceptionThrown()
        {
            var sessionId = 1;
            _mockScheduleService.Setup(s => s.CancelAsync(sessionId))
                .Throws(new Exception("Cancel failed"));

            var result = await _controller.Cancel(sessionId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Cancel failed", badRequestResult.Value);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Cancel_WorksWithDifferentSessionIds(int sessionId)
        {
            _mockScheduleService.Setup(s => s.CancelAsync(sessionId))
                .Returns(Task.CompletedTask);

            var result = await _controller.Cancel(sessionId);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenScheduleServiceNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ScheduleController(null, _mockGroupService.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGroupServiceNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ScheduleController(_mockScheduleService.Object, null));
        }

        #endregion
    }
}
