using System.ComponentModel.DataAnnotations;

public enum StudyTaskType
{
    [Display(Name = "Study session")]
    StudySession,

    [Display(Name = "Lecture")]
    Lecture,

    [Display(Name = "Seminar")]
    Seminar,

    [Display(Name = "Exam")]
    Exam,

    [Display(Name = "Assignment/Project")]
    Assignment,

    [Display(Name = "Reading")]
    Reading,

    [Display(Name = "Presentation")]
    Presentation,

    [Display(Name = "Consultation")]
    Consultation
}
