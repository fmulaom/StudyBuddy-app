namespace StudyBuddy.Web.Models.ViewModels
{
/// <summary>
/// SOLID - S: ViewModel ima samo jednu odgovornost - predstavljati podatke za prikaz grupe s detaljima.
/// </summary>
public class GroupDetailsViewModel
    {
        public StudyGroup Group { get; set; }
        public IReadOnlyList<StudyGroupMember> Members { get; set; } = new List<StudyGroupMember>();
        public IReadOnlyList<StudyResource> Resources { get; set; } = new List<StudyResource>();
        public IReadOnlyList<StudySession> Sessions { get; set; } = new List<StudySession>();
    }
}
