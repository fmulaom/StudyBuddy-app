using StudyBuddy.Web.Models;
using StudyBuddy.Web.Services.Interfaces;

namespace StudyBuddy.Web.Services.ServicesImplementation
{
    /// <summary>
    /// SOLID - S: Samo upravlja grupama. Raspored, notifikacije i resursi su odvojeni servisi.
    /// SOLID - DIP: Koristi interfejse, ne konkretne klase.
    /// </summary>
    public class StudyGroupService : IStudyGroupService
    {
        private readonly IRepository<StudyGroup> _groupRepository;
        private readonly IRepository<StudyGroupMember> _memberRepository;
        private readonly ILogger<StudyGroupService> _logger;

        public StudyGroupService(
            IRepository<StudyGroup> groupRepository,
            IRepository<StudyGroupMember> memberRepository,
            ILogger<StudyGroupService> logger)
        {
            _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<StudyGroup>> GetUserGroupsAsync(string userId)
        {
            // SOLID - S: Pronalazi samo grupe korisnika
            var userGroups = await _memberRepository.GetWhereAsync(x => x.UserId == userId);
            var groupIds = userGroups.Select(x => x.GroupId).ToList();

            var groups = new List<StudyGroup>();
            foreach (var id in groupIds)
            {
                var group = await _groupRepository.GetByIdAsync(id);
                if (group != null)
                    groups.Add(group);
            }

            return groups.AsReadOnly();
        }

        public async Task<StudyGroup> GetGroupByIdAsync(int groupId)
        {
            return await _groupRepository.GetByIdAsync(groupId);
        }

        /// <summary>
        /// SOLID - S: Samo kreira grupu. 
        /// Dodavanje članova je odvojena metoda - jedna odgovornost po metodi.
        /// </summary>
        public async Task<StudyGroup> CreateGroupAsync(string ownerId, string name)
        {
            var group = new StudyGroup
            {
                Name = name,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveChangesAsync();

            // SOLID - S: Odmah dodaj vlasnika kao člana
            var ownerMember = new StudyGroupMember
            {
                GroupId = group.Id,
                UserId = ownerId,
                Role = "Owner",
                JoinedAt = DateTime.UtcNow
            };

            await _memberRepository.AddAsync(ownerMember);
            await _memberRepository.SaveChangesAsync();

            _logger.LogInformation($"Grupa '{name}' kreirana od {ownerId}.");
            return group;
        }

        /// <summary>
        /// SOLID - S: Samo dodavanje člana.
        /// </summary>
        public async Task AddMemberAsync(int groupId, string userId, string role = "Member")
        {
            // SOLID - DIP: Koristi apstrakciju za validaciju
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException("Grupa nije pronađena.");

            var existingMember = await _memberRepository.GetWhereAsync(x =>
                x.GroupId == groupId && x.UserId == userId);

            if (existingMember.Any())
                throw new InvalidOperationException("Korisnik je već član grupe.");

            var member = new StudyGroupMember
            {
                GroupId = groupId,
                UserId = userId,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            await _memberRepository.AddAsync(member);
            await _memberRepository.SaveChangesAsync();

            _logger.LogInformation($"Korisnik {userId} dodan u grupu {groupId} s ulogom {role}.");
        }

        /// <summary>
        /// SOLID - S: Samo brisanje člana.
        /// </summary>
        public async Task RemoveMemberAsync(int groupId, string userId)
        {
            var members = await _memberRepository.GetWhereAsync(x =>
                x.GroupId == groupId && x.UserId == userId);

            if (!members.Any())
                throw new KeyNotFoundException("Član nije pronađen.");

            await _memberRepository.DeleteAsync(members.First());

            _logger.LogInformation($"Korisnik {userId} uklonjen iz grupe {groupId}.");
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                throw new KeyNotFoundException("Grupa nije pronađena.");

            await _groupRepository.DeleteAsync(group);
        }
    }
}
