using UsosFix.Models;

namespace UsosFix.ViewModels;

public record SubjectOverview(int Id, LanguageString Name, Semester Semester)
{
    public SubjectOverview(Subject model) : this(model.Id, model.Name, new Semester(model.Semester)) { }
}