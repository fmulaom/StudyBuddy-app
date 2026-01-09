using StudyBuddy.Web.Models;
using StudyBuddy.Web.Models.ViewModels;
using Xunit;

namespace StudyBuddy.Test.Models.ViewModels
{
    public class EditStudyProfileViewModelTests
    {
        [Fact]
        public void EditStudyProfileViewModel_CanBeCreated()
        {
            var model = new EditStudyProfileViewModel
            {
                Subject = "Mathematics",
                Faculty = "FER",
                Level = "Advanced"
            };

            Assert.Equal("Mathematics", model.Subject);
            Assert.Equal("FER", model.Faculty);
            Assert.Equal("Advanced", model.Level);
        }

        [Theory]
        [InlineData("Mathematics")]
        [InlineData("Physics")]
        [InlineData("Chemistry")]
        public void EditStudyProfileViewModel_Subject_CanHaveDifferentValues(string subject)
        {
            var model = new EditStudyProfileViewModel { Subject = subject };

            Assert.Equal(subject, model.Subject);
        }

        [Theory]
        [InlineData("FER")]
        [InlineData("FF")]
        [InlineData("PMF")]
        public void EditStudyProfileViewModel_Faculty_CanHaveDifferentValues(string faculty)
        {
            var model = new EditStudyProfileViewModel { Faculty = faculty };

            Assert.Equal(faculty, model.Faculty);
        }

        [Theory]
        [InlineData("Beginner")]
        [InlineData("Intermediate")]
        [InlineData("Advanced")]
        public void EditStudyProfileViewModel_Level_CanHaveDifferentValues(string level)
        {
            var model = new EditStudyProfileViewModel { Level = level };

            Assert.Equal(level, model.Level);
        }
    }

    public class PartnerSearchViewModelTests
    {
        [Fact]
        public void PartnerSearchViewModel_CanBeCreated()
        {
            var model = new PartnerSearchViewModel
            {
                Subject = "Math",
                Faculty = "Science",
                Level = "Advanced",
                Results = new List<StudyPartner>()
            };

            Assert.Equal("Math", model.Subject);
            Assert.Equal("Science", model.Faculty);
            Assert.Equal("Advanced", model.Level);
            Assert.Empty(model.Results);
        }

        [Fact]
        public void PartnerSearchViewModel_Results_CanBePopulated()
        {
            var results = new List<StudyPartner>
            {
                new StudyPartner
                {
                    Id = 1,
                    Subject = "Math",
                    Faculty = "Science",
                    Level = "Advanced",
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                },
                new StudyPartner
                {
                    Id = 2,
                    Subject = "Physics",
                    Faculty = "Science",
                    Level = "Intermediate",
                    IsFavorite = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            var model = new PartnerSearchViewModel { Results = results };

            Assert.Equal(2, model.Results.Count);
        }

        [Fact]
        public void PartnerSearchViewModel_CanHaveNullResults()
        {
            var model = new PartnerSearchViewModel { Results = null };

            Assert.Null(model.Results);
        }

        [Fact]
        public void PartnerSearchViewModel_CanHaveEmptyResults()
        {
            var model = new PartnerSearchViewModel { Results = new List<StudyPartner>() };

            Assert.Empty(model.Results);
        }
    }

    public class ScheduleSessionViewModelTests
    {
        [Fact]
        public void ScheduleSessionViewModel_CanBeCreated()
        {
            var now = DateTime.UtcNow;
            var model = new ScheduleSessionViewModel
            {
                GroupId = 1,
                StartTime = now,
                EndTime = now.AddHours(2),
                Location = "Room 101"
            };

            Assert.Equal(1, model.GroupId);
            Assert.Equal(now, model.StartTime);
            Assert.Equal(now.AddHours(2), model.EndTime);
            Assert.Equal("Room 101", model.Location);
        }

        [Fact]
        public void ScheduleSessionViewModel_GroupId_CanBeSet()
        {
            var model = new ScheduleSessionViewModel { GroupId = 42 };

            Assert.Equal(42, model.GroupId);
        }

        [Theory]
        [InlineData("Room 101")]
        [InlineData("Online Teams")]
        [InlineData("Library")]
        public void ScheduleSessionViewModel_Location_CanHaveDifferentValues(string location)
        {
            var model = new ScheduleSessionViewModel { Location = location };

            Assert.Equal(location, model.Location);
        }

        [Fact]
        public void ScheduleSessionViewModel_StartTime_CanBeSet()
        {
            var time = DateTime.UtcNow.AddDays(1);
            var model = new ScheduleSessionViewModel { StartTime = time };

            Assert.Equal(time, model.StartTime);
        }

        [Fact]
        public void ScheduleSessionViewModel_EndTime_CanBeSet()
        {
            var time = DateTime.UtcNow.AddDays(1).AddHours(2);
            var model = new ScheduleSessionViewModel { EndTime = time };

            Assert.Equal(time, model.EndTime);
        }

        [Fact]
        public void ScheduleSessionViewModel_EndTime_CanBeAfterStartTime()
        {
            var start = DateTime.UtcNow;
            var end = start.AddHours(3);
            var model = new ScheduleSessionViewModel
            {
                StartTime = start,
                EndTime = end
            };

            Assert.True(model.EndTime > model.StartTime);
        }
    }

    public class CreateGroupViewModelTests
    {
        [Fact]
        public void CreateGroupViewModel_CanBeCreated()
        {
            var model = new CreateGroupViewModel { Name = "C# Study Group" };

            Assert.Equal("C# Study Group", model.Name);
        }

        [Fact]
        public void CreateGroupViewModel_CanHaveEmptyName()
        {
            var model = new CreateGroupViewModel { Name = "" };

            Assert.Equal("", model.Name);
        }

        [Fact]
        public void CreateGroupViewModel_CanHaveNullName()
        {
            var model = new CreateGroupViewModel { Name = null };

            Assert.Null(model.Name);
        }

        [Theory]
        [InlineData("Math Group")]
        [InlineData("Physics Study Circle")]
        [InlineData("Chemistry Lab")]
        public void CreateGroupViewModel_Name_CanHaveDifferentValues(string name)
        {
            var model = new CreateGroupViewModel { Name = name };

            Assert.Equal(name, model.Name);
        }

        [Fact]
        public void CreateGroupViewModel_Name_CanBeLongString()
        {
            var longName = new string('a', 500);
            var model = new CreateGroupViewModel { Name = longName };

            Assert.Equal(500, model.Name.Length);
        }
    }

    public class GroupDetailsViewModelTests
    {
        [Fact]
        public void GroupDetailsViewModel_CanBeCreated()
        {
            var group = new StudyGroup
            {
                Id = 1,
                Name = "Test Group",
                OwnerId = "user1",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var members = new List<StudyGroupMember>();
            var sessions = new List<StudySession>();

            var model = new GroupDetailsViewModel
            {
                Group = group,
                Members = members,
                Sessions = sessions
            };

            Assert.Equal(group, model.Group);
            Assert.Equal(members, model.Members);
            Assert.Equal(sessions, model.Sessions);
        }

        [Fact]
        public void GroupDetailsViewModel_Group_CanBeSet()
        {
            var group = new StudyGroup
            {
                Id = 5,
                Name = "C# Group",
                OwnerId = "owner123",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            var model = new GroupDetailsViewModel { Group = group };

            Assert.Equal(5, model.Group.Id);
            Assert.Equal("C# Group", model.Group.Name);
        }

        [Fact]
        public void GroupDetailsViewModel_Members_CanBePopulated()
        {
            var members = new List<StudyGroupMember>
            {
                new StudyGroupMember
                {
                    Id = 1,
                    GroupId = 1,
                    UserId = "user1",
                    Role = "Admin",
                    JoinedAt = DateTime.UtcNow
                },
                new StudyGroupMember
                {
                    Id = 2,
                    GroupId = 1,
                    UserId = "user2",
                    Role = "Member",
                    JoinedAt = DateTime.UtcNow
                }
            };
            var model = new GroupDetailsViewModel { Members = members };

            Assert.Equal(2, model.Members.Count);
        }

        [Fact]
        public void GroupDetailsViewModel_Sessions_CanBePopulated()
        {
            var sessions = new List<StudySession>
            {
                new StudySession
                {
                    Id = 1,
                    GroupId = 1,
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow.AddHours(2),
                    Location = "Room 101",
                    Status = "Scheduled",
                    CreatedAt = DateTime.UtcNow
                }
            };
            var model = new GroupDetailsViewModel { Sessions = sessions };

            Assert.Single(model.Sessions);
        }

        [Fact]
        public void GroupDetailsViewModel_CanHaveEmptyMembers()
        {
            var model = new GroupDetailsViewModel { Members = new List<StudyGroupMember>() };

            Assert.Empty(model.Members);
        }

        [Fact]
        public void GroupDetailsViewModel_CanHaveEmptySessions()
        {
            var model = new GroupDetailsViewModel { Sessions = new List<StudySession>() };

            Assert.Empty(model.Sessions);
        }
    }

    public class WeeklyCalendarViewModelTests
    {
        [Fact]
        public void WeeklyCalendarViewModel_CanBeCreated()
        {
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            var model = new WeeklyCalendarViewModel
            {
                WeekStart = weekStart,
                TasksByDay = new Dictionary<DayOfWeek, List<StudyTasks>>()
            };

            Assert.Equal(weekStart, model.WeekStart);
            Assert.NotNull(model.TasksByDay);
        }

        [Fact]
        public void WeeklyCalendarViewModel_WeekStart_CanBeSet()
        {
            var date = new DateTime(2024, 1, 8);
            var model = new WeeklyCalendarViewModel { WeekStart = date };

            Assert.Equal(date, model.WeekStart);
        }

        [Fact]
        public void WeeklyCalendarViewModel_TasksByDay_CanHaveMultipleDays()
        {
            var tasksByDay = new Dictionary<DayOfWeek, List<StudyTasks>>
            {
                { DayOfWeek.Monday, new List<StudyTasks>() },
                { DayOfWeek.Tuesday, new List<StudyTasks>() },
                { DayOfWeek.Wednesday, new List<StudyTasks>() }
            };
            var model = new WeeklyCalendarViewModel { TasksByDay = tasksByDay };

            Assert.Equal(3, model.TasksByDay.Count);
        }

        [Fact]
        public void WeeklyCalendarViewModel_TasksByDay_CanContainTasks()
        {
            var tasks = new List<StudyTasks>
            {
                new StudyTasks
                {
                    Id = 1,
                    Title = "Task 1",
                    TaskType = StudyTaskType.StudySession,
                    EstimatedMinutes = 60
                }
            };
            var tasksByDay = new Dictionary<DayOfWeek, List<StudyTasks>>
            {
                { DayOfWeek.Monday, tasks }
            };
            var model = new WeeklyCalendarViewModel { TasksByDay = tasksByDay };

            Assert.Single(model.TasksByDay[DayOfWeek.Monday]);
        }

        [Fact]
        public void WeeklyCalendarViewModel_CanHaveMultipleTasksOnSameDay()
        {
            var tasks = new List<StudyTasks>
            {
                new StudyTasks { Id = 1, Title = "Task 1", EstimatedMinutes = 30 },
                new StudyTasks { Id = 2, Title = "Task 2", EstimatedMinutes = 45 },
                new StudyTasks { Id = 3, Title = "Task 3", EstimatedMinutes = 60 }
            };
            var tasksByDay = new Dictionary<DayOfWeek, List<StudyTasks>>
            {
                { DayOfWeek.Monday, tasks }
            };
            var model = new WeeklyCalendarViewModel { TasksByDay = tasksByDay };

            Assert.Equal(3, model.TasksByDay[DayOfWeek.Monday].Count);
        }

        [Fact]
        public void WeeklyCalendarViewModel_TasksByDay_CanBeEmpty()
        {
            var tasksByDay = new Dictionary<DayOfWeek, List<StudyTasks>>();
            var model = new WeeklyCalendarViewModel { TasksByDay = tasksByDay };

            Assert.Empty(model.TasksByDay);
        }
    }
}
