using StudyBuddy.Web.Models;
namespace StudyBuddy.Web.Services.Interfaces;
/// <summary>
/// SOLID - DIP (Dependency Inversion Principle):
/// Controlleri ne ovise o konkretnoj implementaciji, već o ovom interfejsu.
/// SOLID - ISP (Interface Segregation Principle):
/// Interfejs je mali i fokusiran samo na podudaranje partnera, ne na sve servise.
/// </summary>
public interface IPartnerMatchingService
{
    /// <summary>
    /// Pronalazi automatske podudarke za korisnika.
    /// </summary>
    Task<IReadOnlyList<StudyPartner>> GetAutomaticMatchesAsync(string userId);

    /// <summary>
    /// Pretraživanje partnera s filterima.
    /// </summary>
    Task<IReadOnlyList<StudyPartner>> SearchPartnersAsync(string subject, string faculty, string level);

    /// <summary>
    /// Označava partnera kao omiljenog.
    /// </summary>
    Task MarkFavoriteAsync(string userId, int partnerId);
}