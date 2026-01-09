using StudyBuddy.Web.Models;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class StudyGroupMemberNavigationTests
    {
        [Fact]
        public void StudyGroupMember_Group_CanBeSetAndRead()
        {
            var group = new StudyGroup { Id = 5, Name = "Physics" };
            var member = new StudyGroupMember
            {
                Id = 1,
                GroupId = 5,
                Group = group
            };

            Assert.Equal(group, member.Group);
            Assert.Equal(5, member.GroupId);
        }
    }
}
