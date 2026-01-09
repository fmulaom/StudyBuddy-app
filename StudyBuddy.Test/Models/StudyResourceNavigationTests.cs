using StudyBuddy.Web.Models;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class StudyResourceNavigationTests
    {
        [Fact]
        public void StudyResource_Group_CanBeSetAndRead()
        {
            var group = new StudyGroup { Id = 10, Name = "Test Group" };
            var resource = new StudyResource
            {
                Id = 1,
                GroupId = 10,
                Group = group
            };

            Assert.Equal(group, resource.Group);
            Assert.Equal(10, resource.GroupId);
        }
    }
}
