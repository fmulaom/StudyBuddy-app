using System.ComponentModel.DataAnnotations;

namespace StudyBuddy.Web.Models.ViewModels
{
    /// <summary>
    /// SOLID - S: Samo dodavanje člana.
    /// </summary>
    public class AddMemberViewModel
    {
        [Required]
        public string UserId { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "Member";
    }
}
