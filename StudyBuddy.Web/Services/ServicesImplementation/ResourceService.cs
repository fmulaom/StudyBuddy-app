using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.ServicesImplementation
{
    /// <summary>
    /// SOLID - S: Samo dijeljenje resursa.
    /// </summary>
    public class ResourceService : IResourceService
    {
        private readonly IRepository<StudyResource> _resourceRepository;
        private readonly IRepository<StudyGroup> _groupRepository;
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(
            IRepository<StudyResource> resourceRepository,
            IRepository<StudyGroup> groupRepository,
            ILogger<ResourceService> logger)
        {
            _resourceRepository = resourceRepository ?? throw new ArgumentNullException(nameof(resourceRepository));
            _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<StudyResource>> GetGroupResourcesAsync(int groupId)
        {
            return await _resourceRepository.GetWhereAsync(x => x.GroupId == groupId);
        }

        /// <summary>
        /// SOLID - S: Samo dodavanje resursa.
        /// </summary>
        public async Task<StudyResource> UploadResourceAsync(int groupId, string title, string url, string type, string uploadedBy)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException("Grupa nije pronađena.");

            var resource = new StudyResource
            {
                GroupId = groupId,
                Title = title,
                Url = url,
                Type = type,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            await _resourceRepository.AddAsync(resource);
            await _resourceRepository.SaveChangesAsync();

            _logger.LogInformation($"Resurs '{title}' dodan u grupu {groupId}.");
            return resource;
        }

        public async Task DeleteResourceAsync(int resourceId)
        {
            var resource = await _resourceRepository.GetByIdAsync(resourceId);
            if (resource == null)
                throw new KeyNotFoundException("Resurs nije pronađen.");

            await _resourceRepository.DeleteAsync(resource);
        }
    }
}
