using System.Security.Cryptography;
using System.Text;

namespace StudyBuddy.Web.Services.Logging;

public static class LogValue
{
    public static string Safe(string? value, int maxLen = 64)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
            if (!char.IsControl(ch)) sb.Append(ch);

        return sb.Length <= maxLen ? sb.ToString() : sb.ToString(0, maxLen);
    }

    public static string UserToken(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return "anonymous";

        var cleaned = Safe(userId);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(cleaned));
        return Convert.ToHexString(hash)[..12];
    }
}
