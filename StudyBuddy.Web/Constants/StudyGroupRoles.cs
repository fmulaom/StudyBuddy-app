using System.Collections.ObjectModel;

namespace StudyBuddy.Web.Constants;

public static class StudyGroupRoles
{
    public const string Member = "Member";
    public const string Admin = "Admin";
    public const string Owner = "Owner";

    private static readonly HashSet<string> AllowedSet = new(StringComparer.OrdinalIgnoreCase)
    {
        Member, Admin, Owner
    };

    public static IReadOnlyCollection<string> Allowed { get; } =
        new ReadOnlyCollection<string>(AllowedSet.ToList());

    public static bool IsAllowed(string? role) =>
        !string.IsNullOrWhiteSpace(role) && AllowedSet.Contains(role);
}
