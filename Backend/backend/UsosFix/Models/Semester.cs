namespace UsosFix.Models;

public record Semester(SemesterSeason Season, int Year, bool IsCurrent, int Id);

public enum SemesterSeason
{
    Winter,
    Summer
}