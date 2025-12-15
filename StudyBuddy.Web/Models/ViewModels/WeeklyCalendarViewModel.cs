namespace StudyBuddy.Web.Models.ViewModels
{
    public class WeeklyCalendarViewModel
    {
        public DateTime WeekStart { get; set; }

        public Dictionary<DayOfWeek, List<StudyTasks>> TasksByDay { get; set; }
            = new Dictionary<DayOfWeek, List<StudyTasks>>();
    }
}
