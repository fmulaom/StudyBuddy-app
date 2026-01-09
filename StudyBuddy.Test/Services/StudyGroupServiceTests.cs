using Moq;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.ServicesImplementation;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace StudyBuddy.Test.Services
{
    public class StudyGroupServiceTests
    {
        private readonly Mock<IRepository<StudyGroup>> _mockGroupRepository;
        private readonly Mock<IRepository<StudyGroupMember>> _mockMemberRepository;
        private readonly Mock<ILogger<StudyGroupService>> _mockLogger;
        private readonly StudyGroupService _service;

        public StudyGroupServiceTests()
        {
            _mockGroupRepository = new Mock<IRepository<StudyGroup>>();
            _mockMemberRepository = new Mock<IRepository<StudyGroupMember>>();
            _mockLogger = new Mock<ILogger<StudyGroupService>>();
            _service = new StudyGroupService(_mockGroupRepository.Object, _mockMemberRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUserGroupsAsync_ReturnsUserGroups()
        {
            var userId = "user1";
            var members = new List<StudyGroupMember>
            {
                new StudyGroupMember { Id = 1, GroupId = 1, UserId = userId, Role = "Member", JoinedAt = DateTime.UtcNow },
                new StudyGroupMember { Id = 2, GroupId = 2, UserId = userId, Role = "Owner", JoinedAt = DateTime.UtcNow }
            };

            var groups = new List<StudyGroup>
            {
                new StudyGroup { Id = 1, Name = "Math Group", OwnerId = "owner1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new StudyGroup { Id = 2, Name = "Physics Group", OwnerId = userId, IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            _mockMemberRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(members);

            _mockGroupRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(groups[0]);
            _mockGroupRepository.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(groups[1]);

            var result = await _service.GetUserGroupsAsync(userId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUserGroupsAsync_ReturnsEmpty_WhenUserHasNoGroups()
        {
            _mockMemberRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(new List<StudyGroupMember>());

            var result = await _service.GetUserGroupsAsync("user1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetGroupByIdAsync_ReturnsGroup()
        {
            var group = new StudyGroup { Id = 1, Name = "Math Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(group);

            var result = await _service.GetGroupByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Math Group", result.Name);
        }

        [Fact]
        public async Task GetGroupByIdAsync_ReturnsNull_WhenGroupNotFound()
        {
            _mockGroupRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudyGroup)null);

            var result = await _service.GetGroupByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateGroupAsync_CreatesGroup_AndAddsOwnerAsMember()
        {
            var ownerId = "user1";
            var groupName = "New Group";
            StudyGroup capturedGroup = null;
            StudyGroupMember capturedMember = null;

            _mockGroupRepository.Setup(r => r.AddAsync(It.IsAny<StudyGroup>()))
                .Callback<StudyGroup>(g => { capturedGroup = g; g.Id = 1; })
                .Returns(Task.CompletedTask);

            _mockGroupRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            _mockMemberRepository.Setup(r => r.AddAsync(It.IsAny<StudyGroupMember>()))
                .Callback<StudyGroupMember>(m => capturedMember = m)
                .Returns(Task.CompletedTask);

            _mockMemberRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.CreateGroupAsync(ownerId, groupName);

            Assert.NotNull(result);
            Assert.Equal(groupName, result.Name);
            Assert.Equal(ownerId, result.OwnerId);
            Assert.True(result.IsActive);

            _mockGroupRepository.Verify(r => r.AddAsync(It.IsAny<StudyGroup>()), Times.Once);
            _mockMemberRepository.Verify(r => r.AddAsync(It.IsAny<StudyGroupMember>()), Times.Once);
        }

        [Fact]
        public async Task CreateGroupAsync_OwnerRoleIsSet()
        {
            var ownerId = "user1";
            StudyGroupMember capturedMember = null;

            _mockGroupRepository.Setup(r => r.AddAsync(It.IsAny<StudyGroup>()))
                .Callback<StudyGroup>(g => g.Id = 1)
                .Returns(Task.CompletedTask);

            _mockGroupRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            _mockMemberRepository.Setup(r => r.AddAsync(It.IsAny<StudyGroupMember>()))
                .Callback<StudyGroupMember>(m => capturedMember = m)
                .Returns(Task.CompletedTask);

            _mockMemberRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.CreateGroupAsync(ownerId, "Group");

            Assert.NotNull(capturedMember);
            Assert.Equal("Owner", capturedMember.Role);
            Assert.Equal(ownerId, capturedMember.UserId);
        }

        [Fact]
        public async Task AddMemberAsync_AddsMemberToGroup()
        {
            var groupId = 1;
            var userId = "user2";
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockMemberRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(new List<StudyGroupMember>());
            _mockMemberRepository.Setup(r => r.AddAsync(It.IsAny<StudyGroupMember>())).Returns(Task.CompletedTask);
            _mockMemberRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.AddMemberAsync(groupId, userId);

            _mockMemberRepository.Verify(r => r.AddAsync(It.IsAny<StudyGroupMember>()), Times.Once);
        }

        [Fact]
        public async Task AddMemberAsync_ThrowsException_WhenGroupNotFound()
        {
            _mockGroupRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudyGroup)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.AddMemberAsync(999, "user1"));
        }

        [Fact]
        public async Task AddMemberAsync_ThrowsException_WhenMemberAlreadyExists()
        {
            var groupId = 1;
            var userId = "user2";
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };
            var existingMember = new StudyGroupMember { Id = 1, GroupId = groupId, UserId = userId, Role = "Member", JoinedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockMemberRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(new List<StudyGroupMember> { existingMember });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddMemberAsync(groupId, userId));
        }

        [Fact]
        public async Task RemoveMemberAsync_RemovesMember()
        {
            var groupId = 1;
            var userId = "user2";
            var member = new StudyGroupMember { Id = 1, GroupId = groupId, UserId = userId, Role = "Member", JoinedAt = DateTime.UtcNow };

            _mockMemberRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(new List<StudyGroupMember> { member });
            _mockMemberRepository.Setup(r => r.DeleteAsync(member)).Returns(Task.CompletedTask);

            await _service.RemoveMemberAsync(groupId, userId);

            _mockMemberRepository.Verify(r => r.DeleteAsync(member), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberAsync_ThrowsException_WhenMemberNotFound()
        {
            _mockMemberRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyGroupMember, bool>>>()))
                .ReturnsAsync(new List<StudyGroupMember>());

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveMemberAsync(1, "user1"));
        }

        [Fact]
        public async Task DeleteGroupAsync_DeletesGroup()
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockGroupRepository.Setup(r => r.DeleteAsync(group)).Returns(Task.CompletedTask);

            await _service.DeleteGroupAsync(groupId);

            _mockGroupRepository.Verify(r => r.DeleteAsync(group), Times.Once);
        }

        [Fact]
        public async Task DeleteGroupAsync_ThrowsException_WhenGroupNotFound()
        {
            _mockGroupRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudyGroup)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteGroupAsync(999));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGroupRepositoryNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyGroupService(null, _mockMemberRepository.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMemberRepositoryNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyGroupService(_mockGroupRepository.Object, null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyGroupService(_mockGroupRepository.Object, _mockMemberRepository.Object, null));
        }
    }
}
