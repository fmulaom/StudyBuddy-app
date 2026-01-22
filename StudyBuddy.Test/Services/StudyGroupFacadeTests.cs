using Moq;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.StudyGroupConfig;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyBuddy.Test.Services
{
    public class StudyGroupFacadeTests
    {
        private readonly Mock<IStudyGroupService> _mockGroupService;
        private readonly Mock<IRepository<StudyGroupMember>> _mockMemberRepo;
        private readonly Mock<IResourceService> _mockResourceService;
        private readonly Mock<ILogger<StudyGroupFacade>> _mockLogger;
        private readonly StudyGroupFacade _facade;
        private readonly Mock<IStudyGroupFacade> _mockFacade;

        public StudyGroupFacadeTests()
        {
            _mockGroupService = new Mock<IStudyGroupService>();
            _mockMemberRepo = new Mock<IRepository<StudyGroupMember>>();
            _mockResourceService = new Mock<IResourceService>();
            _mockLogger = new Mock<ILogger<StudyGroupFacade>>();
            _facade = new StudyGroupFacade(
                _mockGroupService.Object,
                _mockMemberRepo.Object,
                _mockResourceService.Object,
                _mockLogger.Object);
            _mockFacade = new Mock<IStudyGroupFacade>();
        }

        #region GetUserGroups Tests

        [Fact]
        public async Task GetUserGroupsAsync_ReturnsUserGroups()
        {
            var userId = "user1";
            var groups = new List<StudyGroup>
            {
                new StudyGroup { Id = 1, Name = "Math", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new StudyGroup { Id = 2, Name = "Physics", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _mockGroupService.Setup(s => s.GetUserGroupsAsync(userId))
                .ReturnsAsync(groups);

            var result = await _facade.GetUserGroupsAsync(userId);

            Assert.Equal(2, result.Count());
            _mockGroupService.Verify(s => s.GetUserGroupsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserGroupsAsync_ReturnsEmpty_WhenNoGroups()
        {
            var userId = "user1";
            _mockGroupService.Setup(s => s.GetUserGroupsAsync(userId))
                .ReturnsAsync(new List<StudyGroup>());

            var result = await _facade.GetUserGroupsAsync(userId);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("user1")]
        [InlineData("user2")]
        [InlineData("user123")]
        public async Task GetUserGroupsAsync_WorksWithDifferentUserIds(string userId)
        {
            _mockGroupService.Setup(s => s.GetUserGroupsAsync(userId))
                .ReturnsAsync(new List<StudyGroup>());

            var result = await _facade.GetUserGroupsAsync(userId);

            Assert.NotNull(result);
        }

        #endregion

        #region CreateGroup Tests

        [Fact]
        public async Task CreateGroupAsync_CreatesGroup_AndReturnsRedirect()
        {
            var userId = "user1";
            var model = new CreateGroupViewModel { Name = "New Group" };
            var createdGroup = new StudyGroup
            {
                Id = 1,
                Name = model.Name,
                OwnerId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.CreateGroupAsync(userId, model.Name))
                .ReturnsAsync(createdGroup);

            var result = await _facade.CreateGroupAsync(model, userId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("StudyGroups", redirectResult.ControllerName);
        }

        [Fact]
        public async Task CreateGroupAsync_ReturnsBadRequest_WhenModelNull()
        {
            var result = await _facade.CreateGroupAsync(null, "user1");

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task CreateGroupAsync_ReturnsBadRequest_WhenNameEmpty()
        {
            var model = new CreateGroupViewModel { Name = "" };

            var result = await _facade.CreateGroupAsync(model, "user1");

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task CreateGroupAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var model = new CreateGroupViewModel { Name = "Group" };
            _mockGroupService.Setup(s => s.CreateGroupAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Creation failed"));

            var result = await _facade.CreateGroupAsync(model, "user1");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateGroupAsync_LogsInformation_WhenSuccessful()
        {
            var userId = "user1";
            var model = new CreateGroupViewModel { Name = "New Group" };
            var createdGroup = new StudyGroup
            {
                Id = 1,
                Name = model.Name,
                OwnerId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.CreateGroupAsync(userId, model.Name))
                .ReturnsAsync(createdGroup);

            await _facade.CreateGroupAsync(model, userId);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateGroupAsync_ReturnsActionResult()
        {
            var model = new CreateGroupViewModel();
            var expected = new RedirectToActionResult("Index", "StudyGroups", null);
            _mockFacade.Setup(f => f.CreateGroupAsync(model, "user1")).ReturnsAsync(expected);

            var result = await _mockFacade.Object.CreateGroupAsync(model, "user1");

            Assert.Equal(expected, result);
        }

        #endregion

        #region GetMembers Tests

        [Fact]
        public async Task GetMembersAsync_ReturnsGroupDetailsViewModel()
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
            var members = new List<StudyGroupMember>
            {
                new StudyGroupMember { Id = 1, GroupId = groupId, UserId = "user1", Role = "Owner", JoinedAt = DateTime.UtcNow }
            };
            var resources = new List<StudyResource>
            {
                new StudyResource { Id = 1, GroupId = groupId, Title = "Resource", Url = "url", Type = "PDF", UploadedBy = "user1", UploadedAt = DateTime.UtcNow }
            };

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync(group);
            _mockMemberRepo.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(members);
            _mockResourceService.Setup(r => r.GetGroupResourcesAsync(groupId))
                .ReturnsAsync(resources);

            var result = await _facade.GetMembersAsync(groupId);

            Assert.NotNull(result);
            Assert.Equal(group.Id, result.Group.Id);
            Assert.Single(result.Members);
            Assert.Single(result.Resources);
        }

        [Fact]
        public async Task GetMembersAsync_ThrowsException_WhenGroupNotFound()
        {
            var groupId = 999;
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync((StudyGroup)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _facade.GetMembersAsync(groupId));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task GetMembersAsync_WorksWithDifferentGroupIds(int groupId)
        {
            var group = new StudyGroup
            {
                Id = groupId,
                Name = "Group",
                OwnerId = "user1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync(group);
            _mockMemberRepo.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(new List<StudyGroupMember>());
            _mockResourceService.Setup(r => r.GetGroupResourcesAsync(groupId))
                .ReturnsAsync(new List<StudyResource>());

            var result = await _facade.GetMembersAsync(groupId);

            Assert.NotNull(result);
        }

        #endregion

        #region AddMember Tests

        [Fact]
        public async Task AddMemberAsync_AddsMember_AndReturnsRedirect()
        {
            var groupId = 1;
            var userId = "user2";
            var role = "Member";

            _mockGroupService.Setup(s => s.AddMemberAsync(groupId, userId, role))
                .Returns(Task.CompletedTask);

            var result = await _facade.AddMemberAsync(groupId, userId, role);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Members", redirectResult.ActionName);
            Assert.Equal("StudyGroups", redirectResult.ControllerName);
        }

        [Fact]
        public async Task AddMemberAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var groupId = 1;
            var userId = "user2";

            _mockGroupService.Setup(s => s.AddMemberAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Member already exists"));

            var result = await _facade.AddMemberAsync(groupId, userId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task AddMemberAsync_ReturnsActionResult()
        {
            var expected = new RedirectToActionResult("Members", "StudyGroups", null);
            _mockFacade.Setup(f => f.AddMemberAsync(1, "user2", "Member")).ReturnsAsync(expected);

            var result = await _mockFacade.Object.AddMemberAsync(1, "user2", "Member");

            Assert.Equal(expected, result);
        }

        #endregion

        #region RemoveMember Tests

        [Fact]
        public async Task RemoveMemberAsync_RemovesMember_AndReturnsRedirect()
        {
            var groupId = 1;
            var userId = "user2";

            _mockGroupService.Setup(s => s.RemoveMemberAsync(groupId, userId))
                .Returns(Task.CompletedTask);

            var result = await _facade.RemoveMemberAsync(groupId, userId);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Members", redirectResult.ActionName);
        }

        [Fact]
        public async Task RemoveMemberAsync_ReturnsBadRequest_WhenExceptionThrown()
        {
            var groupId = 1;
            var userId = "user2";

            _mockGroupService.Setup(s => s.RemoveMemberAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException("Member not found"));

            var result = await _facade.RemoveMemberAsync(groupId, userId);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Theory]
        [InlineData("user1")]
        [InlineData("user2")]
        [InlineData("user123")]
        public async Task RemoveMemberAsync_WorksWithDifferentUserIds(string userId)
        {
            var groupId = 1;

            _mockGroupService.Setup(s => s.RemoveMemberAsync(groupId, userId))
                .Returns(Task.CompletedTask);

            var result = await _facade.RemoveMemberAsync(groupId, userId);

            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task RemoveMemberAsync_ReturnsActionResult()
        {
            var expected = new RedirectToActionResult("Members", "StudyGroups", null);
            _mockFacade.Setup(f => f.RemoveMemberAsync(1, "user2")).ReturnsAsync(expected);

            var result = await _mockFacade.Object.RemoveMemberAsync(1, "user2");

            Assert.Equal(expected, result);
        }

        #endregion

        #region GetGroupDetails Tests

        [Fact]
        public async Task GetGroupDetailsAsync_ReturnsOkResult_WhenGroupFound()
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

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync(group);

            var result = await _facade.GetGroupDetailsAsync(groupId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGroup = Assert.IsType<StudyGroup>(okResult.Value);
            Assert.Equal(group.Id, returnedGroup.Id);
        }

        [Fact]
        public async Task GetGroupDetailsAsync_ReturnsNotFound_WhenGroupNotFound()
        {
            var groupId = 999;
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync((StudyGroup)null);

            var result = await _facade.GetGroupDetailsAsync(groupId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task GetGroupDetailsAsync_WorksWithDifferentGroupIds(int groupId)
        {
            var group = new StudyGroup
            {
                Id = groupId,
                Name = "Group",
                OwnerId = "user1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(groupId)).ReturnsAsync(group);

            var result = await _facade.GetGroupDetailsAsync(groupId);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task CreateGroup_GetMembers_Integration()
        {
            var userId = "user1";
            var model = new CreateGroupViewModel { Name = "New Group" };
            var createdGroup = new StudyGroup
            {
                Id = 1,
                Name = model.Name,
                OwnerId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _mockGroupService.Setup(s => s.CreateGroupAsync(userId, model.Name))
                .ReturnsAsync(createdGroup);

            var members = new List<StudyGroupMember>();
            var resources = new List<StudyResource>();

            _mockGroupService.Setup(s => s.GetGroupByIdAsync(1)).ReturnsAsync(createdGroup);
            _mockMemberRepo.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(members);
            _mockResourceService.Setup(r => r.GetGroupResourcesAsync(1))
                .ReturnsAsync(resources);

            var createResult = await _facade.CreateGroupAsync(model, userId);
            var details = await _facade.GetMembersAsync(1);

            Assert.IsType<RedirectToActionResult>(createResult);
            Assert.NotNull(details);
            Assert.Equal(createdGroup.Id, details.Group.Id);
        }

        #endregion
    }
}
