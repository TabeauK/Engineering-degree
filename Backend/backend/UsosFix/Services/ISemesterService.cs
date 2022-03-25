using System.Threading.Tasks;
using UsosFix.Models;

namespace UsosFix.Services;

public interface ISemesterService
{
    Task<Semester> GetCurrentSemesterAsync();
    Task SetCurrentSemesterAsync(int year, SemesterSeason season);
    Task MoveToNextSemesterAsync();
}