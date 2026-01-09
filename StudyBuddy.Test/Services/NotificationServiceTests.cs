using Microsoft.Extensions.Logging;
using Moq;
using StudyBuddy.Web.Services.ServicesImplementation;
using Xunit;

namespace StudyBuddy.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<ILogger<NotificationService>> _mockLogger;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _mockLogger = new Mock<ILogger<NotificationService>>();
            _service = new NotificationService(_mockLogger.Object);
        }

        [Fact]
        public async Task NotifyGroupMembersAsync_LogsInformation()
        {
            await _service.NotifyGroupMembersAsync(42, "Test message");

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task SendSessionReminderAsync_LogsInformation()
        {
            await _service.SendSessionReminderAsync(99);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("99")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_ThrowsIfLoggerNull()
        {
            Assert.Throws<ArgumentNullException>(() => new NotificationService(null));
        }
    }
}
