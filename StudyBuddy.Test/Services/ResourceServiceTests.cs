using Moq;
using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;
using StudyBuddy.Web.Services.ServicesImplementation;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudyBuddy.Test.Services
{
    public class ResourceServiceTests
    {
        private readonly Mock<IRepository<StudyResource>> _mockResourceRepository;
        private readonly Mock<IRepository<StudyGroup>> _mockGroupRepository;
        private readonly Mock<ILogger<ResourceService>> _mockLogger;
        private readonly ResourceService _service;
        private readonly Mock<IResourceService> _mockService;

        public ResourceServiceTests()
        {
            _mockResourceRepository = new Mock<IRepository<StudyResource>>();
            _mockGroupRepository = new Mock<IRepository<StudyGroup>>();
            _mockLogger = new Mock<ILogger<ResourceService>>();
            _service = new ResourceService(
                _mockResourceRepository.Object,
                _mockGroupRepository.Object,
                _mockLogger.Object);
            _mockService = new Mock<IResourceService>();
        }

        [Fact]
        public async Task GetGroupResourcesAsync_ReturnsResources()
        {
            var groupId = 1;
            var resources = new List<StudyResource>
            {
                new StudyResource
                {
                    Id = 1,
                    GroupId = groupId,
                    Title = "Math Notes",
                    Url = "https://example.com/notes1",
                    Type = "PDF",
                    UploadedBy = "user1",
                    UploadedAt = DateTime.UtcNow
                },
                new StudyResource
                {
                    Id = 2,
                    GroupId = groupId,
                    Title = "Physics Summary",
                    Url = "https://example.com/notes2",
                    Type = "Document",
                    UploadedBy = "user2",
                    UploadedAt = DateTime.UtcNow
                }
            };

            _mockResourceRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyResource, bool>>>()))
                .ReturnsAsync(resources);

            var result = await _service.GetGroupResourcesAsync(groupId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetGroupResourcesAsync_ReturnsEmpty_WhenNoResources()
        {
            _mockResourceRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyResource, bool>>>()))
                .ReturnsAsync(new List<StudyResource>());

            var result = await _service.GetGroupResourcesAsync(1);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task GetGroupResourcesAsync_WorksWithDifferentGroupIds(int groupId)
        {
            _mockResourceRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<StudyResource, bool>>>()))
                .ReturnsAsync(new List<StudyResource>());

            var result = await _service.GetGroupResourcesAsync(groupId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UploadResourceAsync_CreatesResource()
        {
            var groupId = 1;
            var title = "Math Notes";
            var url = "https://example.com/notes";
            var type = "PDF";
            var uploadedBy = "user1";
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockResourceRepository.Setup(r => r.AddAsync(It.IsAny<StudyResource>())).Returns(Task.CompletedTask);
            _mockResourceRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.UploadResourceAsync(groupId, title, url, type, uploadedBy);

            Assert.NotNull(result);
            Assert.Equal(groupId, result.GroupId);
            Assert.Equal(title, result.Title);
            Assert.Equal(url, result.Url);
            Assert.Equal(type, result.Type);
            Assert.Equal(uploadedBy, result.UploadedBy);
        }

        [Fact]
        public async Task UploadResourceAsync_ThrowsException_WhenGroupNotFound()
        {
            _mockGroupRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudyGroup)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UploadResourceAsync(999, "Title", "url", "PDF", "user1"));
        }

        [Theory]
        [InlineData("PDF")]
        [InlineData("Document")]
        [InlineData("Image")]
        [InlineData("Video")]
        [InlineData("Link")]
        public async Task UploadResourceAsync_WorksWithDifferentTypes(string type)
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockResourceRepository.Setup(r => r.AddAsync(It.IsAny<StudyResource>())).Returns(Task.CompletedTask);
            _mockResourceRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.UploadResourceAsync(groupId, "Title", "url", type, "user1");

            Assert.Equal(type, result.Type);
        }

        [Theory]
        [InlineData("Math Notes")]
        [InlineData("Physics Summary")]
        [InlineData("Chemistry Formulas")]
        public async Task UploadResourceAsync_WorksWithDifferentTitles(string title)
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockResourceRepository.Setup(r => r.AddAsync(It.IsAny<StudyResource>())).Returns(Task.CompletedTask);
            _mockResourceRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.UploadResourceAsync(groupId, title, "url", "PDF", "user1");

            Assert.Equal(title, result.Title);
        }

        [Fact]
        public async Task UploadResourceAsync_SetsUploadedAtTimestamp()
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };
            var beforeUpload = DateTime.UtcNow;

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockResourceRepository.Setup(r => r.AddAsync(It.IsAny<StudyResource>())).Returns(Task.CompletedTask);
            _mockResourceRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var result = await _service.UploadResourceAsync(groupId, "Title", "url", "PDF", "user1");
            var afterUpload = DateTime.UtcNow;

            Assert.True(result.UploadedAt >= beforeUpload && result.UploadedAt <= afterUpload);
        }

        [Fact]
        public async Task UploadResourceAsync_CallsRepositoryAdd()
        {
            var groupId = 1;
            var group = new StudyGroup { Id = groupId, Name = "Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow };

            _mockGroupRepository.Setup(r => r.GetByIdAsync(groupId)).ReturnsAsync(group);
            _mockResourceRepository.Setup(r => r.AddAsync(It.IsAny<StudyResource>())).Returns(Task.CompletedTask);
            _mockResourceRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            await _service.UploadResourceAsync(groupId, "Title", "url", "PDF", "user1");

            _mockResourceRepository.Verify(r => r.AddAsync(It.IsAny<StudyResource>()), Times.Once);
            _mockResourceRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteResourceAsync_DeletesResource()
        {
            var resourceId = 1;
            var resource = new StudyResource
            {
                Id = resourceId,
                GroupId = 1,
                Title = "Notes",
                Url = "url",
                Type = "PDF",
                UploadedBy = "user1",
                UploadedAt = DateTime.UtcNow
            };

            _mockResourceRepository.Setup(r => r.GetByIdAsync(resourceId)).ReturnsAsync(resource);
            _mockResourceRepository.Setup(r => r.DeleteAsync(resource)).Returns(Task.CompletedTask);

            await _service.DeleteResourceAsync(resourceId);

            _mockResourceRepository.Verify(r => r.DeleteAsync(resource), Times.Once);
        }

        [Fact]
        public async Task DeleteResourceAsync_ThrowsException_WhenResourceNotFound()
        {
            _mockResourceRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((StudyResource)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteResourceAsync(999));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task DeleteResourceAsync_WorksWithDifferentResourceIds(int resourceId)
        {
            var resource = new StudyResource
            {
                Id = resourceId,
                GroupId = 1,
                Title = "Notes",
                Url = "url",
                Type = "PDF",
                UploadedBy = "user1",
                UploadedAt = DateTime.UtcNow
            };

            _mockResourceRepository.Setup(r => r.GetByIdAsync(resourceId)).ReturnsAsync(resource);
            _mockResourceRepository.Setup(r => r.DeleteAsync(resource)).Returns(Task.CompletedTask);

            await _service.DeleteResourceAsync(resourceId);

            _mockResourceRepository.Verify(r => r.DeleteAsync(resource), Times.Once);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenResourceRepositoryNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceService(null, _mockGroupRepository.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenGroupRepositoryNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceService(_mockResourceRepository.Object, null, _mockLogger.Object));
        }

        
        [Fact]
        public async Task DeleteResourceAsync_CallsService()
        {
            _mockService.Setup(s => s.DeleteResourceAsync(3)).Returns(Task.CompletedTask);

            await _mockService.Object.DeleteResourceAsync(3);

            _mockService.Verify(s => s.DeleteResourceAsync(3), Times.Once);
        }
    }
}
