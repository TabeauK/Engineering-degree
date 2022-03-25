using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record Semester(SemesterSeason Season, int Year)
    {
        public Semester(Models.Semester model) : this(model.Season, model.Year) { }
    }
}
