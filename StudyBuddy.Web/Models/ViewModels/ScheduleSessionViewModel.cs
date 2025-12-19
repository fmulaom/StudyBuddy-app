using System.ComponentModel.DataAnnotations;

namespace StudyBuddy.Web.Models.ViewModels
{
    /// <summary>
    /// SOLID - S: Samo podatke za zakazivanje sesije.
    /// </summary>
    public class ScheduleSessionViewModel
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; }
    }
}
