using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using Xunit;

namespace StudyBuddy.Test.ViewModelTests
{
    public class PartnerSearchViewModelTests
    {
        [Fact]
        public void PartnerSearchViewModel_CanBeCreated()
        {
            var vm = new PartnerSearchViewModel
            {
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                Results = new List<StudyPartner>
                {
                    new StudyPartner { Id = 1, UserId = "u1" }
                }
            };

            Assert.Equal("Math", vm.Subject);
            Assert.Equal("Science", vm.Faculty);
            Assert.Equal("Advanced", vm.Level);
            Assert.Single(vm.Results);
        }

        [Fact]
        public void PartnerSearchViewModel_Results_DefaultsToEmptyList()
        {
            var vm = new PartnerSearchViewModel();
            Assert.NotNull(vm.Results);
            Assert.Empty(vm.Results);
        }
    }
}
