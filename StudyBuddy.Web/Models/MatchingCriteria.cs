using StudyBuddy.Web.Models.Enums;

namespace StudyBuddy.Web.Models
{
    public class MatchingCriteria
    {
        public string Subject { get; set; } = string.Empty;
        public string Faculty { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public MatchingMode Mode { get; set; } = MatchingMode.Automatic;
    }
}
