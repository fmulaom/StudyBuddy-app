namespace StudyBuddy.Web.Models;

/// <summary>
/// Predstavlja zakazanu studijsku sesiju.
/// SOLID - S: Modelira samo termin, vrijeme i lokaciju. Obavijesti su odjeljene u drugom servisu.
/// </summary>
public class StudySession
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // SOLID - LSP: Relacija je konzistentna
    public virtual StudyGroup Group { get; set; }
}