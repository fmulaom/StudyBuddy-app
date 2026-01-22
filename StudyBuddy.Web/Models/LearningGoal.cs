using System;
using System.ComponentModel.DataAnnotations;

namespace StudyBuddy.Web.Models
{
    // S - Single Responsibility:
   
    public class LearningGoal
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }
        public DateTime CreatedAt { get; set; }
        [Range(0, 100)]
        public int Progress { get; set; }
    }
}
