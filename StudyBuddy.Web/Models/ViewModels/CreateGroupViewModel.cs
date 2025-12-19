using System.ComponentModel.DataAnnotations;

namespace StudyBuddy.Web.Models.ViewModels
{
    /// <summary>
    /// SOLID - S: Samo kreiranje grupe, nema dodatne logike.
    /// </summary>
    public class CreateGroupViewModel
    {
        [Required(ErrorMessage = "Naziv grupe je obavezan.")]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }
    }
}
