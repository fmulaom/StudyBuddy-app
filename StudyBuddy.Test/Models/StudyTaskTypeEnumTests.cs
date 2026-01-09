using StudyBuddy.Web.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class StudyTaskTypeEnumTests
    {
        [Theory]
        [InlineData(StudyTaskType.StudySession, "Study session")]
        [InlineData(StudyTaskType.Lecture, "Lecture")]
        [InlineData(StudyTaskType.Seminar, "Seminar")]
        [InlineData(StudyTaskType.Exam, "Exam")]
        [InlineData(StudyTaskType.Assignment, "Assignment/Project")]
        [InlineData(StudyTaskType.Reading, "Reading")]
        [InlineData(StudyTaskType.Presentation, "Presentation")]
        [InlineData(StudyTaskType.Consultation, "Consultation")]
        public void StudyTaskType_HasDisplayName(StudyTaskType type, string expectedDisplay)
        {
            var member = typeof(StudyTaskType).GetMember(type.ToString()).First();
            var display = member.GetCustomAttributes(typeof(DisplayAttribute), false)
                                .Cast<DisplayAttribute>().FirstOrDefault();
            Assert.NotNull(display);
            Assert.Equal(expectedDisplay, display.Name);
        }
    }
}
