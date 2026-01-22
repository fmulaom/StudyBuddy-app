namespace StudyBuddy.Web.Constants;

public static class StudyGroupRoles
{
    public const string Member = "Member";
    public const string Admin = "Admin";
    public const string Owner = "Owner";

    public static readonly ISet<string> Allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Member, Admin, Owner
    };

    public static bool IsAllowed(string? role) =>
        !string.IsNullOrWhiteSpace(role) && Allowed.Contains(role);
}
