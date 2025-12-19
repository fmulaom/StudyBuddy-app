namespace StudyBuddy.Web.Models;


/// <summary>
/// Reprezentira profil studenta kao studijskog partnera.
/// SOLID - S (Single Responsibility): Entitet ima samo jednu odgovornost - modelirati partnera.
/// </summary>
public class StudyPartner
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public string Faculty { get; set; }
    public string Subject { get; set; }
    public string Level { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
}