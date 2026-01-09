using StudyBuddy.Web.Models;
using Xunit;

namespace StudyBuddy.Test.Models
{
    public class StudyGroupTests
    {
        [Fact]
        public void StudyGroup_CanBeCreated()
        {
            var group = new StudyGroup
            {
                Id = 1,
                Name = "C# Study Group",
                OwnerId = "user123",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            Assert.Equal(1, group.Id);
            Assert.Equal("C# Study Group", group.Name);
            Assert.Equal("user123", group.OwnerId);
            Assert.True(group.IsActive);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StudyGroup_IsActive_CanHaveDifferentValues(bool isActive)
        {
            var group = new StudyGroup { IsActive = isActive };

            Assert.Equal(isActive, group.IsActive);
        }

        [Fact]
        public void StudyGroup_CanHaveNullName()
        {
            var group = new StudyGroup { Name = null };

            Assert.Null(group.Name);
        }
    }

    public class StudyPartnerTests2
    {
        [Fact]
        public void StudyPartner_CanBeCreated()
        {
            var partner = new StudyPartner
            {
                Id = 1,
                UserId = "user123",
                Subject = "Mathematics",
                Faculty = "Science",
                Level = "Advanced",
                IsFavorite = false,
                CreatedAt = DateTime.UtcNow
            };

            Assert.Equal(1, partner.Id);
            Assert.Equal("user123", partner.UserId);
            Assert.Equal("Mathematics", partner.Subject);
            Assert.Equal("Science", partner.Faculty);
            Assert.Equal("Advanced", partner.Level);
            Assert.False(partner.IsFavorite);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StudyPartner_IsFavorite_CanChange(bool isFavorite)
        {
            var partner = new StudyPartner { IsFavorite = isFavorite };

            Assert.Equal(isFavorite, partner.IsFavorite);
        }

        [Theory]
        [InlineData("Mathematics")]
        [InlineData("Physics")]
        [InlineData("Chemistry")]
        public void StudyPartner_Subject_CanHaveDifferentValues(string subject)
        {
            var partner = new StudyPartner { Subject = subject };

            Assert.Equal(subject, partner.Subject);
        }
    }

    public class StudySessionTests
    {
        [Fact]
        public void StudySession_CanBeCreated()
        {
            var now = DateTime.UtcNow;
            var session = new StudySession
            {
                Id = 1,
                GroupId = 1,
                StartTime = now,
                EndTime = now.AddHours(2),
                Location = "Room 101",
                Status = "Scheduled",
                CreatedAt = now
            };

            Assert.Equal(1, session.Id);
            Assert.Equal(1, session.GroupId);
            Assert.Equal(now, session.StartTime);
            Assert.Equal(now.AddHours(2), session.EndTime);
            Assert.Equal("Room 101", session.Location);
            Assert.Equal("Scheduled", session.Status);
        }

        [Theory]
        [InlineData("Scheduled")]
        [InlineData("Completed")]
        [InlineData("Cancelled")]
        public void StudySession_Status_CanHaveDifferentValues(string status)
        {
            var session = new StudySession { Status = status };

            Assert.Equal(status, session.Status);
        }

        [Fact]
        public void StudySession_EndTime_CanBeAfterStartTime()
        {
            var start = DateTime.UtcNow;
            var end = start.AddHours(3);
            var session = new StudySession
            {
                StartTime = start,
                EndTime = end
            };

            Assert.True(session.EndTime > session.StartTime);
        }
    }

    public class StudyGroupMemberTests
    {
        [Fact]
        public void StudyGroupMember_CanBeCreated()
        {
            var now = DateTime.UtcNow;
            var member = new StudyGroupMember
            {
                Id = 1,
                GroupId = 1,
                UserId = "user123",
                Role = "Admin",
                JoinedAt = now
            };

            Assert.Equal(1, member.Id);
            Assert.Equal(1, member.GroupId);
            Assert.Equal("user123", member.UserId);
            Assert.Equal("Admin", member.Role);
            Assert.Equal(now, member.JoinedAt);
        }

        [Theory]
        [InlineData("Owner")]
        [InlineData("Admin")]
        [InlineData("Member")]
        public void StudyGroupMember_Role_CanHaveDifferentValues(string role)
        {
            var member = new StudyGroupMember { Role = role };

            Assert.Equal(role, member.Role);
        }
    }

    public class StudyResourceTests
    {
        [Fact]
        public void StudyResource_CanBeCreated()
        {
            var now = DateTime.UtcNow;
            var resource = new StudyResource
            {
                Id = 1,
                GroupId = 1,
                Title = "Lecture Notes",
                Url = "https://example.com/notes.pdf",
                Type = "PDF",
                UploadedBy = "user123",
                UploadedAt = now
            };

            Assert.Equal(1, resource.Id);
            Assert.Equal(1, resource.GroupId);
            Assert.Equal("Lecture Notes", resource.Title);
            Assert.Equal("https://example.com/notes.pdf", resource.Url);
            Assert.Equal("PDF", resource.Type);
            Assert.Equal("user123", resource.UploadedBy);
        }

        [Theory]
        [InlineData("PDF")]
        [InlineData("Word")]
        [InlineData("Excel")]
        [InlineData("PowerPoint")]
        public void StudyResource_Type_CanHaveDifferentValues(string type)
        {
            var resource = new StudyResource { Type = type };

            Assert.Equal(type, resource.Type);
        }
    }

    public class StudyTasksTests
    {
        [Fact]
        public void StudyTasks_CanBeCreated()
        {
            var dueDate = DateTime.Today.AddDays(7);
            var task = new StudyTasks
            {
                Id = 1,
                UserId = "user123",
                Title = "Complete Assignment",
                Description = "Math assignment chapter 5",
                TaskType = StudyTaskType.Assignment,
                DueDate = dueDate,
                EstimatedMinutes = 120
            };

            Assert.Equal(1, task.Id);
            Assert.Equal("user123", task.UserId);
            Assert.Equal("Complete Assignment", task.Title);
            Assert.Equal(StudyTaskType.Assignment, task.TaskType);
            Assert.Equal(120, task.EstimatedMinutes);
        }

        [Theory]
        [InlineData(StudyTaskType.StudySession)]
        [InlineData(StudyTaskType.Exam)]
        [InlineData(StudyTaskType.Lecture)]
        [InlineData(StudyTaskType.Assignment)]
        [InlineData(StudyTaskType.Reading)]
        public void StudyTasks_TaskType_CanHaveDifferentValues(StudyTaskType taskType)
        {
            var task = new StudyTasks { TaskType = taskType };

            Assert.Equal(taskType, task.TaskType);
        }

        [Theory]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        [InlineData(240)]
        public void StudyTasks_EstimatedMinutes_CanHaveDifferentValues(int minutes)
        {
            var task = new StudyTasks { EstimatedMinutes = minutes };

            Assert.Equal(minutes, task.EstimatedMinutes);
        }

        [Fact]
        public void StudyTasks_CanHavePastDueDate()
        {
            var task = new StudyTasks { DueDate = DateTime.Today.AddDays(-5) };

            Assert.True(task.DueDate < DateTime.Today);
        }

        [Fact]
        public void StudyTasks_CanHaveFutureDueDate()
        {
            var task = new StudyTasks { DueDate = DateTime.Today.AddDays(30) };

            Assert.True(task.DueDate > DateTime.Today);
        }
    }

    public class ErrorViewModelTests
    {
        [Fact]
        public void ErrorViewModel_CanBeCreated()
        {
            var model = new ErrorViewModel
            {
                RequestId = "request123"
            };

            Assert.Equal("request123", model.RequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ReturnsTrueWhenRequestIdExists()
        {
            var model = new ErrorViewModel { RequestId = "request123" };

            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ReturnsFalseWhenRequestIdIsNull()
        {
            var model = new ErrorViewModel { RequestId = null };

            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_ReturnsFalseWhenRequestIdIsEmpty()
        {
            var model = new ErrorViewModel { RequestId = "" };

            Assert.False(model.ShowRequestId);
        }
    }

    public class ApplicationUserTests
    {
        [Fact]
        public void ApplicationUser_CanBeCreated()
        {
            var user = new ApplicationUser
            {
                Id = "user123",
                UserName = "johndoe",
                Email = "john@example.com",
            };

            Assert.Equal("user123", user.Id);
            Assert.Equal("johndoe", user.UserName);
            Assert.Equal("john@example.com", user.Email);
        }
    }

    public class MultipleStudyGroupsTests
    {
        [Fact]
        public void Multiple_StudyGroups_CanExist()
        {
            var groups = new List<StudyGroup>
            {
                new StudyGroup { Id = 1, Name = "Math Group", OwnerId = "user1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new StudyGroup { Id = 2, Name = "Physics Group", OwnerId = "user2", IsActive = true, CreatedAt = DateTime.UtcNow },
                new StudyGroup { Id = 3, Name = "Chemistry Group", OwnerId = "user3", IsActive = false, CreatedAt = DateTime.UtcNow }
            };

            Assert.Equal(3, groups.Count);
            Assert.Equal(2, groups.Count(g => g.IsActive));
        }

       
        [Fact]
        public void Multiple_StudySessions_CanExist()
        {
            var sessions = new List<StudySession>
            {
                new StudySession { Id = 1, GroupId = 1, Status = "Scheduled", CreatedAt = DateTime.UtcNow },
                new StudySession { Id = 2, GroupId = 1, Status = "Completed", CreatedAt = DateTime.UtcNow },
                new StudySession { Id = 3, GroupId = 2, Status = "Cancelled", CreatedAt = DateTime.UtcNow }
            };

            Assert.Equal(3, sessions.Count);
            Assert.Equal(2, sessions.Count(s => s.GroupId == 1));
            Assert.Single(sessions.Where(s => s.Status == "Completed"));
        }
    }
}
