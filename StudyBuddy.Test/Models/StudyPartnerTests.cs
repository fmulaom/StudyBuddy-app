using StudyBuddy.Web.Models;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class StudyPartnerTests
    {
        [Fact]
        public void CanCreateStudyPartner()
        {
            var partner = new StudyPartner
            {
                Id = 1,
                UserId = "user",
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced"
            };
            Assert.Equal("Math", partner.Subject);
        }
    }
}
