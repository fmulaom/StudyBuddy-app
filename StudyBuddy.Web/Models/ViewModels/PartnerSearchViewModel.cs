namespace StudyBuddy.Web.Models.ViewModels
{
    /// <summary>
    /// SOLID - S: ViewModel ima samo jednu odgovornost - predstavljati podatke za prikaz.
    /// Nije model baze, nije servisna logika.
    /// </summary>
    public class PartnerSearchViewModel
    {
        public string Subject { get; set; }
        public string Faculty { get; set; }
        public string Level { get; set; }
        public IReadOnlyList<StudyPartner> Results { get; set; } = new List<StudyPartner>();
    }
}
