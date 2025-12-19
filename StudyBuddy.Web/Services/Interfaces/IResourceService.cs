using StudyBuddy.Web.Models;

namespace StudyBuddy.Web.Services.Interfaces;

/// <summary>
/// SOLID - ISP: Razdvojena od grupe i rasporeda.
/// SOLID - DIP: Abstraktna, lako se može zamijeniti druga implementacija.
/// </summary>
public interface IResourceService
{
    Task<IReadOnlyList<StudyResource>> GetGroupResourcesAsync(int groupId);
    Task<StudyResource> UploadResourceAsync(int groupId, string title, string url, string type, string uploadedBy);
    Task DeleteResourceAsync(int resourceId);
}
