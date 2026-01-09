using Microsoft.AspNetCore.Http;
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
    public class StudyGroupsControllerTests
    {
        private readonly Mock<IStudyGroupFacade> _mockFacade;
        private readonly Mock<IStudyGroupService> _mockGroupService;
        private readonly StudyGroupsController _controller;
        private const string TestUserId = "user-123";

        public StudyGroupsControllerTests()
        {
            _mockFacade = new Mock<IStudyGroupFacade>();
            _mockGroupService = new Mock<IStudyGroupService>();
            _controller = new StudyGroupsController(_mockFacade.Object, _mockGroupService.Object);

            SetupUserClaims(TestUserId);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithGroups_WhenGroupsExist()
        {
            var groups = new List<StudyGroup>
            {
                new StudyGroup { Id = 1, Name = "Group 1", OwnerId = TestUserId, CreatedAt = DateTime.UtcNow, IsActive = true },
                new StudyGroup { Id = 2, Name = "Group 2", OwnerId = TestUserId, CreatedAt = DateTime.UtcNow, IsActive = true }
            };
            _mockFacade.Setup(f => f.GetUserGroupsAsync(TestUserId)).ReturnsAsync(groups);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StudyGroup>>(viewResult.Model);
            Assert.Equal(2, model.Count());
            _mockFacade.Verify(f => f.GetUserGroupsAsync(TestUserId), Times.Once);
        }

        [Fact]
        public async Task Index_ReturnsViewWithEmptyList_WhenNoGroups()
        {
            _mockFacade.Setup(f => f.GetUserGroupsAsync(TestUserId)).ReturnsAsync(new List<StudyGroup>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<StudyGroup>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task Index_CallsFacadeWithCorrectUserId()
        {
            _mockFacade.Setup(f => f.GetUserGroupsAsync(It.IsAny<string>())).ReturnsAsync(new List<StudyGroup>());

            await _controller.Index();

            _mockFacade.Verify(f => f.GetUserGroupsAsync(TestUserId), Times.Once);
        }

        #endregion

        #region Create GET Tests

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsEmptyModel()
        {
            var result = _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_CallsFacadeWithCorrectParameters()
        {
            var model = new CreateGroupViewModel { Name = "C# Study Group" };
            var expectedResult = new RedirectToActionResult("Index", "StudyGroups", null);
            _mockFacade.Setup(f => f.CreateGroupAsync(model, TestUserId)).ReturnsAsync(expectedResult);

            var result = await _controller.Create(model);

            Assert.IsType<RedirectToActionResult>(result);
            _mockFacade.Verify(f => f.CreateGroupAsync(It.Is<CreateGroupViewModel>(m =>
                m.Name == "C# Study Group"
            ), TestUserId), Times.Once);
        }

        [Fact]
        public async Task Create_Post_RedirectsToIndex_WhenSuccessful()
        {
            var model = new CreateGroupViewModel { Name = "Test Group" };
            var expectedResult = new RedirectToActionResult("Index", "StudyGroups", null);
            _mockFacade.Setup(f => f.CreateGroupAsync(It.IsAny<CreateGroupViewModel>(), It.IsAny<string>())).ReturnsAsync(expectedResult);

            var result = await _controller.Create(model);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_PassesUserIdToFacade()
        {
            var model = new CreateGroupViewModel { Name = "Group" };
            _mockFacade.Setup(f => f.CreateGroupAsync(It.IsAny<CreateGroupViewModel>(), It.IsAny<string>())).ReturnsAsync(new RedirectToActionResult("Index", "StudyGroups", null));

            await _controller.Create(model);

            _mockFacade.Verify(f => f.CreateGroupAsync(It.IsAny<CreateGroupViewModel>(), TestUserId), Times.Once);
        }

        [Theory]
        [InlineData("Math Group")]
        [InlineData("Physics Group")]
        [InlineData("Chemistry Group")]
        public async Task Create_Post_WorksWithDifferentGroupNames(string name)
        {
            var model = new CreateGroupViewModel { Name = name };
            _mockFacade.Setup(f => f.CreateGroupAsync(It.IsAny<CreateGroupViewModel>(), It.IsAny<string>())).ReturnsAsync(new RedirectToActionResult("Index", "StudyGroups", null));

            var result = await _controller.Create(model);

            Assert.IsType<RedirectToActionResult>(result);
        }

        #endregion

        #region Details Tests

        [Fact]
        public async Task Details_ReturnsViewWithGroup_WhenGroupExists()
        {
            var group = new StudyGroup
            {
                Id = 1,
                Name = "C# Developers",
                OwnerId = TestUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(1)).ReturnsAsync(group);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudyGroup>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("C# Developers", model.Name);
            _mockGroupService.Verify(s => s.GetGroupByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task Details_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(999)).ReturnsAsync((StudyGroup)null);

            var result = await _controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Details_WorksWithDifferentGroupIds(int groupId)
        {
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = TestUserId, CreatedAt = DateTime.UtcNow, IsActive = true };
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync(group);

            var result = await _controller.Details(groupId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StudyGroup>(viewResult.Model);
            Assert.Equal(groupId, model.Id);
        }

        #endregion

        #region Members Tests

        [Fact]
        public async Task Members_ReturnsViewWithGroupMembers()
        {
            var members = new List<StudyGroupMember>
            {
                new StudyGroupMember { Id = 1, UserId = "user1", GroupId = 1, Role = "Admin", JoinedAt = DateTime.UtcNow },
                new StudyGroupMember { Id = 2, UserId = "user2", GroupId = 1, Role = "Member", JoinedAt = DateTime.UtcNow }
            };
            var groupDetails = new GroupDetailsViewModel
            {
                Group = new StudyGroup { Id = 1, Name = "Group", OwnerId = TestUserId, CreatedAt = DateTime.UtcNow, IsActive = true },
                Members = members
            };
            _mockFacade.Setup(f => f.GetMembersAsync(1)).ReturnsAsync(groupDetails);

            var result = await _controller.Members(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupDetailsViewModel>(viewResult.Model);
            Assert.Equal(1, model.Group.Id);
            Assert.Equal(2, model.Members.Count);
        }

        [Fact]
        public async Task Members_ReturnsViewWithEmptyMembers_WhenGroupHasNoMembers()
        {
            var groupDetails = new GroupDetailsViewModel
            {
                Group = new StudyGroup { Id = 1, Name = "Group", OwnerId = TestUserId, CreatedAt = DateTime.UtcNow, IsActive = true },
                Members = new List<StudyGroupMember>()
            };
            _mockFacade.Setup(f => f.GetMembersAsync(1)).ReturnsAsync(groupDetails);

            var result = await _controller.Members(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupDetailsViewModel>(viewResult.Model);
            Assert.Empty(model.Members);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task Members_WorksWithDifferentGroupIds(int groupId)
        {
            var groupDetails = new GroupDetailsViewModel
            {
                Group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = TestUserId, CreatedAt = DateTime.UtcNow, IsActive = true },
                Members = new List<StudyGroupMember>()
            };
            _mockFacade.Setup(f => f.GetMembersAsync(groupId)).ReturnsAsync(groupDetails);

            var result = await _controller.Members(groupId);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<GroupDetailsViewModel>(viewResult.Model);
            Assert.Equal(groupId, model.Group.Id);
        }

        #endregion

        #region AddMember Tests

        [Fact]
        public async Task AddMember_CallsFacadeWithCorrectParameters()
        {
            int groupId = 1;
            string userId = "new-user";
            string role = "Member";
            var expectedResult = new RedirectToActionResult("Members", "StudyGroups", new { id = groupId });
            _mockFacade.Setup(f => f.AddMemberAsync(groupId, userId, role)).ReturnsAsync(expectedResult);

            var result = await _controller.AddMember(groupId, userId, role);

            _mockFacade.Verify(f => f.AddMemberAsync(groupId, userId, role), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task AddMember_RedirectsToMembers()
        {
            _mockFacade.Setup(f => f.AddMemberAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RedirectToActionResult("Members", "StudyGroups", null));

            var result = await _controller.AddMember(1, "user", "Member");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Members", redirectResult.ActionName);
        }

        [Theory]
        [InlineData("Member")]
        [InlineData("Admin")]
        [InlineData("Moderator")]
        public async Task AddMember_WorksWithDifferentRoles(string role)
        {
            _mockFacade.Setup(f => f.AddMemberAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RedirectToActionResult("Members", "StudyGroups", null));

            var result = await _controller.AddMember(1, "user", role);

            _mockFacade.Verify(f => f.AddMemberAsync(1, "user", role), Times.Once);
        }

        [Fact]
        public async Task AddMember_UsesDefaultRoleIfNotProvided()
        {
            _mockFacade.Setup(f => f.AddMemberAsync(It.IsAny<int>(), It.IsAny<string>(), "Member"))
                .ReturnsAsync(new RedirectToActionResult("Members", "StudyGroups", null));

            var result = await _controller.AddMember(1, "user");

            _mockFacade.Verify(f => f.AddMemberAsync(1, "user", "Member"), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(50)]
        public async Task AddMember_WorksWithDifferentGroupIds(int groupId)
        {
            _mockFacade.Setup(f => f.AddMemberAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new RedirectToActionResult("Members", "StudyGroups", null));

            await _controller.AddMember(groupId, "user", "Member");

            _mockFacade.Verify(f => f.AddMemberAsync(groupId, "user", "Member"), Times.Once);
        }

        #endregion

        #region RemoveMember Tests

        [Fact]
        public async Task RemoveMember_CallsFacadeWithCorrectParameters()
        {
            int groupId = 1;
            string userId = "user-to-remove";
            var expectedResult = new RedirectToActionResult("Members", "StudyGroups", new { id = groupId });
            _mockFacade.Setup(f => f.RemoveMemberAsync(groupId, userId)).ReturnsAsync(expectedResult);

            var result = await _controller.RemoveMember(groupId, userId);

            _mockFacade.Verify(f => f.RemoveMemberAsync(groupId, userId), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task RemoveMember_RedirectsToMembers()
        {
            _mockFacade.Setup(f => f.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new RedirectToActionResult("Members", "StudyGroups", null));

            var result = await _controller.RemoveMember(1, "user");

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Members", redirectResult.ActionName);
        }

        [Theory]
        [InlineData(1, "user1")]
        [InlineData(5, "user5")]
        [InlineData(100, "user100")]
        public async Task RemoveMember_WorksWithDifferentUserIds(int groupId, string userId)
        {
            _mockFacade.Setup(f => f.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new RedirectToActionResult("Members", "StudyGroups", null));

            var result = await _controller.RemoveMember(groupId, userId);

            _mockFacade.Verify(f => f.RemoveMemberAsync(groupId, userId), Times.Once);
        }

        #endregion

        #region Constructor Validation Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFacadeIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyGroupsController(null, _mockGroupService.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyGroupsController(_mockFacade.Object, null));
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
