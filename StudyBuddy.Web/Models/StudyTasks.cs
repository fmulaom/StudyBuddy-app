
namespace StudyBuddy.Web.Models
{
    public class StudyTasks
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public int EstimatedMinutes { get; set; }
        public StudyTaskType TaskType { get; set; }
        public string? Description { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
