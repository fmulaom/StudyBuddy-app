namespace StudyBuddy.Web.Models;

/// <summary>
/// Predstavlja dijeljeni resurs (datoteka, poveznica, bilješke).
/// SOLID - S: Samo modelira resurs, ne upravlja skladištenjem ili dozvolama.
/// </summary>
public class StudyResource
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }
    public string Type { get; set; }
    public string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }

    // SOLID - LSP: Konzistentna navigacija
    public virtual StudyGroup Group { get; set; }
}