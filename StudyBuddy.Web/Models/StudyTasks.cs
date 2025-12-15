
using System.ComponentModel.DataAnnotations;

namespace StudyBuddy.Web.Models
{
    public class StudyTasks
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd.MMM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }
        [Display(Name = "Estimated minutes")]
        public int EstimatedMinutes { get; set; }
        [Display(Name ="Task type")]
        public StudyTaskType TaskType { get; set; }
        public string? Description { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
