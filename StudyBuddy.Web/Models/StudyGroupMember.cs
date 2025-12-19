namespace StudyBuddy.Web.Models;
/// <summary>
/// Predstavlja člana grupe s specifičnom ulogom.
/// SOLID - S: Entitet modelira samo odnos korisnika i grupe, nema poslovanja logike.
/// </summary>
public class StudyGroupMember
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string UserId { get; set; }
    public string Role { get; set; }
    public DateTime JoinedAt { get; set; }

    // SOLID - LSP: Konzistentna navigacijska svojstva
    public virtual StudyGroup Group { get; set; } = null!;
}