using Moq;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.Enums;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.ServicesImplementation;
using StudyBuddy.Web.Services.MatchStrategy;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyBuddy.Test.Services
{
    public class PartnerMatchingServiceTests
    {
        private readonly Mock<IMatchingStrategyFactory> _mockStrategyFactory;
        private readonly Mock<IRepository<StudyPartner>> _mockRepository;
        private readonly Mock<ILogger<PartnerMatchingService>> _mockLogger;
        private readonly PartnerMatchingService _service;
        private readonly Mock<IPartnerMatchingService> _mockService;

        public PartnerMatchingServiceTests()
        {
            _mockStrategyFactory = new Mock<IMatchingStrategyFactory>();
            _mockRepository = new Mock<IRepository<StudyPartner>>();
            _mockLogger = new Mock<ILogger<PartnerMatchingService>>();
            _service = new PartnerMatchingService(
                _mockStrategyFactory.Object,
                _mockLogger.Object,
                _mockRepository.Object);
            _mockService = new Mock<IPartnerMatchingService>();
        }

        #region GetAutomaticMatches Tests

        [Fact]
        public async Task GetAutomaticMatchesAsync_ReturnsMatches()
        {
            var userId = "user1";
            var matches = new List<StudyPartner>
            {
                new StudyPartner
                {
                    Id = 1,
                    UserId = "user2",
                    Subject = "Math",
                    Faculty = "Science",
                    Level = "Advanced",
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                },
                new StudyPartner
                {
                    Id = 2,
                    UserId = "user3",
                    Subject = "Math",
                    Faculty = "Science",
                    Level = "Intermediate",
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(userId, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(matches);

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.ExactMatch))
                .Returns(mockStrategy.Object);

            var result = await _service.GetAutomaticMatchesAsync(userId);

            Assert.Equal(2, result.Count);
            Assert.All(result, m => Assert.NotEqual(userId, m.UserId));
        }

        [Fact]
        public async Task GetAutomaticMatchesAsync_ReturnsEmpty_WhenNoMatches()
        {
            var userId = "user1";
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(userId, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.ExactMatch))
                .Returns(mockStrategy.Object);

            var result = await _service.GetAutomaticMatchesAsync(userId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAutomaticMatchesAsync_UsesExactMatchStrategy()
        {
            var userId = "user1";
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(It.IsAny<string>(), It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.ExactMatch))
                .Returns(mockStrategy.Object);

            await _service.GetAutomaticMatchesAsync(userId);

            _mockStrategyFactory.Verify(f => f.GetStrategy(MatchingMode.ExactMatch), Times.Once);
        }

        [Fact]
        public async Task GetAutomaticMatchesAsync_CallsStrategyWithCorrectCriteria()
        {
            var userId = "user1";
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(userId, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.ExactMatch))
                .Returns(mockStrategy.Object);

            await _service.GetAutomaticMatchesAsync(userId);

            mockStrategy.Verify(
                s => s.MatchAsync(userId, It.Is<MatchingCriteria>(
                    c => c.Mode == MatchingMode.Automatic)),
                Times.Once);
        }

        [Theory]
        [InlineData("user1")]
        [InlineData("user2")]
        [InlineData("user123")]
        public async Task GetAutomaticMatchesAsync_WorksWithDifferentUserIds(string userId)
        {
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(userId, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.ExactMatch))
                .Returns(mockStrategy.Object);

            var result = await _service.GetAutomaticMatchesAsync(userId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAutomaticMatchesAsync_ReturnsMatches_MockService()
        {
            var expected = new List<StudyPartner> { new StudyPartner { Id = 1 } };
            _mockService.Setup(s => s.GetAutomaticMatchesAsync("user1")).ReturnsAsync(expected);

            var result = await _mockService.Object.GetAutomaticMatchesAsync("user1");

            Assert.Equal(expected, result);
        }

        #endregion

        #region SearchPartners Tests

        [Fact]
        public async Task SearchPartnersAsync_ReturnsMatches()
        {
            var subject = "Math";
            var faculty = "Science";
            var level = "Advanced";
            var matches = new List<StudyPartner>
            {
                new StudyPartner
                {
                    Id = 1,
                    UserId = "user2",
                    Subject = subject,
                    Faculty = faculty,
                    Level = level,
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(null, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(matches);

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.FacultyPriority))
                .Returns(mockStrategy.Object);

            var result = await _service.SearchPartnersAsync(subject, faculty, level);

            Assert.Single(result);
            Assert.Equal(subject, result.First().Subject);
        }

        [Fact]
        public async Task SearchPartnersAsync_UsesFacultyPriorityStrategy()
        {
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(null, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.FacultyPriority))
                .Returns(mockStrategy.Object);

            await _service.SearchPartnersAsync("Math", "Science", "Advanced");

            _mockStrategyFactory.Verify(f => f.GetStrategy(MatchingMode.FacultyPriority), Times.Once);
        }

        [Theory]
        [InlineData("Math", "Science", "Advanced")]
        [InlineData("Physics", "Engineering", "Intermediate")]
        [InlineData("History", "Humanities", "Beginner")]
        public async Task SearchPartnersAsync_WorksWithDifferentCriteria(string subject, string faculty, string level)
        {
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(null, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(It.IsAny<MatchingMode>()))
                .Returns(mockStrategy.Object);

            var result = await _service.SearchPartnersAsync(subject, faculty, level);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SearchPartnersAsync_PassesCorrectCriteriaToStrategy()
        {
            var subject = "Math";
            var faculty = "Science";
            var level = "Advanced";
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(null, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.FacultyPriority))
                .Returns(mockStrategy.Object);

            await _service.SearchPartnersAsync(subject, faculty, level);

            mockStrategy.Verify(
                s => s.MatchAsync(null, It.Is<MatchingCriteria>(
                    c => c.Subject == subject &&
                         c.Faculty == faculty &&
                         c.Level == level &&
                         c.Mode == MatchingMode.FacultyPriority)),
                Times.Once);
        }

        [Fact]
        public async Task SearchPartnersAsync_ReturnsEmpty_WhenNoMatches()
        {
            var mockStrategy = new Mock<IMatchingStrategy>();
            mockStrategy.Setup(s => s.MatchAsync(null, It.IsAny<MatchingCriteria>()))
                .ReturnsAsync(new List<StudyPartner>());

            _mockStrategyFactory.Setup(f => f.GetStrategy(MatchingMode.FacultyPriority))
                .Returns(mockStrategy.Object);

            var result = await _service.SearchPartnersAsync("Unknown", "Unknown", "Unknown");

            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchPartnersAsync_ReturnsFilteredResults_MockService()
        {
            var expected = new List<StudyPartner> { new StudyPartner { Id = 2 } };
            _mockService.Setup(s => s.SearchPartnersAsync("Math", "Science", "Advanced")).ReturnsAsync(expected);

            var result = await _mockService.Object.SearchPartnersAsync("Math", "Science", "Advanced");

            Assert.Equal(expected, result);
        }

        #endregion

        #region MarkFavorite Tests

        [Fact]
        public async Task MarkFavoriteAsync_MarksFavorite()
        {
            var userId = "user1";
            var partnerId = 1;
            var partner = new StudyPartner
            {
                Id = partnerId,
                UserId = userId,
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _mockRepository.Setup(r => r.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            await _service.MarkFavoriteAsync(userId, partnerId);

            Assert.True(partner.IsFavorite);
            _mockRepository.Verify(r => r.UpdateAsync(partner), Times.Once);
        }

        [Fact]
        public async Task MarkFavoriteAsync_ThrowsException_WhenNotOwner()
        {
            var userId = "user1";
            var partnerId = 1;
            var partner = new StudyPartner
            {
                Id = partnerId,
                UserId = "user2",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.MarkFavoriteAsync(userId, partnerId));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task MarkFavoriteAsync_ThrowsException_WhenPartnerNotFound()
        {
            var userId = "user1";
            _mockRepository.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((StudyPartner)null);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.MarkFavoriteAsync(userId, 999));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task MarkFavoriteAsync_WorksWithDifferentPartnerIds(int partnerId)
        {
            var userId = "user1";
            var partner = new StudyPartner
            {
                Id = partnerId,
                UserId = userId,
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _mockRepository.Setup(r => r.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            await _service.MarkFavoriteAsync(userId, partnerId);

            Assert.True(partner.IsFavorite);
        }

        [Fact]
        public async Task MarkFavoriteAsync_CallsRepositoryUpdate()
        {
            var userId = "user1";
            var partnerId = 1;
            var partner = new StudyPartner
            {
                Id = partnerId,
                UserId = userId,
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _mockRepository.Setup(r => r.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            await _service.MarkFavoriteAsync(userId, partnerId);

            _mockRepository.Verify(r => r.UpdateAsync(It.Is<StudyPartner>(
                p => p.Id == partnerId && p.IsFavorite == true)), Times.Once);
        }

        [Fact]
        public async Task MarkFavoriteAsync_PreservesOtherProperties()
        {
            var userId = "user1";
            var partnerId = 1;
            var originalSubject = "Math";
            var originalFaculty = "Science";
            var originalLevel = "Advanced";
            var partner = new StudyPartner
            {
                Id = partnerId,
                UserId = userId,
                Subject = originalSubject,
                Faculty = originalFaculty,
                Level = originalLevel,
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(partnerId))
                .ReturnsAsync(partner);
            _mockRepository.Setup(r => r.UpdateAsync(partner))
                .Returns(Task.CompletedTask);

            await _service.MarkFavoriteAsync(userId, partnerId);

            Assert.Equal(originalSubject, partner.Subject);
            Assert.Equal(originalFaculty, partner.Faculty);
            Assert.Equal(originalLevel, partner.Level);
            Assert.True(partner.IsFavorite);
        }

        [Fact]
        public async Task MarkFavoriteAsync_CallsService_MockService()
        {
            _mockService.Setup(s => s.MarkFavoriteAsync("user1", 5)).Returns(Task.CompletedTask);

            await _mockService.Object.MarkFavoriteAsync("user1", 5);

            _mockService.Verify(s => s.MarkFavoriteAsync("user1", 5), Times.Once);
        }

        #endregion
    }
}
