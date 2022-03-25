using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;

namespace UsosFix.Services;

public class SemesterService : ISemesterService
{
    public SemesterService(ApplicationDbContext dbContext) => DbContext = dbContext;
    private ApplicationDbContext DbContext { get; }

    public async Task<Semester> GetCurrentSemesterAsync()
    {
        var current = await DbContext.Semesters.FirstOrDefaultAsync(s => s.IsCurrent);
        if (current is null)
        {
            throw new NoCurrentSemesterException();
        }

        return current;
    }

    public async Task SetCurrentSemesterAsync(int year, SemesterSeason season)
    {
        var oldValue = await GetCurrentSemesterAsync();
        var newValue = new Semester(season, year, true, 0);
        if (oldValue != newValue)
        {
            var existing = AlreadyExists(newValue);
            if (existing is not null)
            {
                DbContext.Entry(existing).State = EntityState.Detached;
                existing = existing with { IsCurrent = true };
                DbContext.Update(existing);
            }
            else
            {
                await DbContext.AddAsync(newValue);
            }

            DbContext.Entry(oldValue).State = EntityState.Detached;
            DbContext.Update(oldValue with { IsCurrent = false });
            await DbContext.SaveChangesAsync();
        }
    }

    public async Task MoveToNextSemesterAsync()
    {
        var current = await GetCurrentSemesterAsync();

        var (newYear, newSeason) = current.Season switch
        {
            SemesterSeason.Summer => (current.Year + 1, SemesterSeason.Winter),
            SemesterSeason.Winter => (current.Year, SemesterSeason.Summer),
            _ => throw new ArgumentOutOfRangeException()
        };

        await SetCurrentSemesterAsync(newYear, newSeason);
    }

    private Semester? AlreadyExists(Semester semester)
    {
        return DbContext.Semesters
            .SingleOrDefault(s => s.Year == semester.Year && s.Season == semester.Season);
    }
}

public class NoCurrentSemesterException : Exception { }