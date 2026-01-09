using Moq;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.ServicesImplementation;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace StudyBuddy.Test.Services
{
    public class StudyScheduleServiceTests
    {
        private readonly Mock<IRepository<StudySession>> _mockSessionRepository;
        private readonly Mock<IRepository<StudyGroup>> _mockGroupRepository;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILogger<StudyScheduleService>> _mockLogger;
        private readonly StudyScheduleService _service;

        public StudyScheduleServiceTests()
        {
            _mockSessionRepository = new Mock<IRepository<StudySession>>();
            _mockGroupRepository = new Mock<IRepository<StudyGroup>>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<StudyScheduleService>>();
            _service = new StudyScheduleService(
                _mockSessionRepository.Object,
                _mockGroupRepository.Object,
                _mockNotificationService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetSessionsForGroupAsync_ReturnsSessions()
        {
            var groupId = 1;
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
                },
                new StudySession
                {
                    Id = 2,
                    GroupId = groupId,
                    StartTime = DateTime.UtcNow.AddDays(2),
                    EndTime = DateTime.UtcNow.AddDays(2).AddHours(2),
                    Location = "Room 102",
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockSessionRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudySession, bool>>>()))
                .ReturnsAsync(sessions);

            var result = await _service.GetSessionsForGroupAsync(groupId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetSessionsForGroupAsync_ReturnsEmpty_WhenNoSessions()
        {
            _mockSessionRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudySession, bool>>>()))
                .ReturnsAsync(new List<StudySession>());

            var result = await _service.GetSessionsForGroupAsync(1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task ScheduleAsync_CreatesSession()
        {
            var groupId = 1;
            var startTime = DateTime.UtcNow.AddDays(1);
            var endTime = DateTime.UtcNow.AddDays(1).AddHours(2);
            var location = "Room 101";
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockSessionRepository.Setup(r => r.AddAsync(It.IsAny<StudySession>())).Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.NotifyGroupMembersAsync(groupId, It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await _service.ScheduleAsync(groupId, startTime, endTime, location);

            Assert.NotNull(result);
            Assert.Equal(groupId, result.GroupId);
            Assert.Equal(location, result.Location);
            Assert.Equal("Scheduled", result.Status);
        }

        [Fact]
        public async Task ScheduleAsync_SendsNotification()
        {
            var groupId = 1;
            var startTime = DateTime.UtcNow.AddDays(1);
            var endTime = DateTime.UtcNow.AddDays(1).AddHours(2);
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockSessionRepository.Setup(r => r.AddAsync(It.IsAny<StudySession>())).Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.NotifyGroupMembersAsync(groupId, It.IsAny<string>())).Returns(Task.CompletedTask);

            await _service.ScheduleAsync(groupId, startTime, endTime, "Room 101");

            _mockNotificationService.Verify(
                n => n.NotifyGroupMembersAsync(groupId, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task ScheduleAsync_ThrowsException_WhenGroupNotFound()
        {
            _mockGroupRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudyGroup)null);

            var startTime = DateTime.UtcNow.AddDays(1);
            var endTime = startTime.AddHours(2);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ScheduleAsync(999, startTime, endTime, "Room 101"));
        }

        [Fact]
        public async Task ScheduleAsync_ThrowsException_WhenStartTimeAfterEndTime()
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };
            var startTime = DateTime.UtcNow.AddDays(2);
            var endTime = DateTime.UtcNow.AddDays(1);

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ScheduleAsync(groupId, startTime, endTime, "Room 101"));
        }

        [Fact]
        public async Task ScheduleAsync_ThrowsException_WhenStartTimeEqualsEndTime()
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };
            var time = DateTime.UtcNow.AddDays(1);

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.ScheduleAsync(groupId, time, time, "Room 101"));
        }

        [Theory]
        [InlineData("Room 101")]
        [InlineData("Library")]
        [InlineData("Online")]
        public async Task ScheduleAsync_WorksWithDifferentLocations(string location)
        {
            var groupId = 1;
            var startTime = DateTime.UtcNow.AddDays(1);
            var endTime = startTime.AddHours(2);
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockSessionRepository.Setup(r => r.AddAsync(It.IsAny<StudySession>())).Returns(Task.CompletedTask);
            _mockSessionRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.NotifyGroupMembersAsync(groupId, It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await _service.ScheduleAsync(groupId, startTime, endTime, location);

            Assert.Equal(location, result.Location);
        }

        [Fact]
        public async Task CancelAsync_CancelsSession()
        {
            var sessionId = 1;
            var session = new StudySession
            {
                Id = sessionId,
                GroupId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room 101",
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _mockSessionRepository.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);
            _mockSessionRepository.Setup(r => r.UpdateAsync(session)).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.NotifyGroupMembersAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            await _service.CancelAsync(sessionId);

            Assert.Equal("Cancelled", session.Status);
            _mockSessionRepository.Verify(r => r.UpdateAsync(session), Times.Once);
        }

        [Fact]
        public async Task CancelAsync_SendsNotification()
        {
            var sessionId = 1;
            var session = new StudySession
            {
                Id = sessionId,
                GroupId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room 101",
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _mockSessionRepository.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);
            _mockSessionRepository.Setup(r => r.UpdateAsync(session)).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.NotifyGroupMembersAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            await _service.CancelAsync(sessionId);

            _mockNotificationService.Verify(
                n => n.NotifyGroupMembersAsync(1, It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task CancelAsync_ThrowsException_WhenSessionNotFound()
        {
            _mockSessionRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudySession)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CancelAsync(999));
        }

        [Fact]
        public async Task CompleteAsync_CompletesSession()
        {
            var sessionId = 1;
            var session = new StudySession
            {
                Id = sessionId,
                GroupId = 1,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                Location = "Room 101",
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _mockSessionRepository.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);
            _mockSessionRepository.Setup(r => r.UpdateAsync(session)).Returns(Task.CompletedTask);

            await _service.CompleteAsync(sessionId);

            Assert.Equal("Completed", session.Status);
            _mockSessionRepository.Verify(r => r.UpdateAsync(session), Times.Once);
        }

        [Fact]
        public async Task CompleteAsync_ThrowsException_WhenSessionNotFound()
        {
            _mockSessionRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudySession)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CompleteAsync(999));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenSessionRepositoryNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyScheduleService(null, _mockGroupRepository.Object, _mockNotificationService.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGroupRepositoryNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyScheduleService(_mockSessionRepository.Object, null, _mockNotificationService.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenNotificationServiceNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyScheduleService(_mockSessionRepository.Object, _mockGroupRepository.Object, null, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new StudyScheduleService(_mockSessionRepository.Object, _mockGroupRepository.Object, _mockNotificationService.Object, null));
        }
    }
}
