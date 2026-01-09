using StudyBuddy.Web.Models.ViewModels;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class EditStudyProfileViewModelTests
    {
        [Fact]
        public void CanSetProperties()
        {
            var model = new EditStudyProfileViewModel
            {
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced"
            };
            Assert.Equal("Math", model.Subject);
        }
    }
}
