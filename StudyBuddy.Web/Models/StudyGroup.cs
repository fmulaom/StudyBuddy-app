namespace StudyBuddy.Web.Models;
public class StudyGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // SOLID - LSP (Liskov Substitution): Relacije su konzistentne i zamjenjive
    public virtual ICollection<StudyGroupMember> Members { get; set; } = new List<StudyGroupMember>();
    public virtual ICollection<StudyResource> Resources { get; set; } = new List<StudyResource>();
    public virtual ICollection<StudySession> Sessions { get; set; } = new List<StudySession>();
}