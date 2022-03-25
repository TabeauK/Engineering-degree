using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using UsosFix.ExchangeRealization;
using UsosFix.Models;

namespace UsosFix.Services;

public class TimetableService
{
    public TimetableService(ApplicationDbContext dbContext, IExchangeSolver exchangeSolver)
    {
        DbContext = dbContext;
        ExchangeSolver = exchangeSolver;
    }

    private ApplicationDbContext DbContext { get; }
    private IExchangeSolver ExchangeSolver { get; }

    public void MakeExchanges(int subjectId)
    {
        var groups = DbContext.Groups
            .Include("Students")
            .Include("Subject")
            .Include("Subject.Semester")
            .Where(g => g.Subject.Id == subjectId);
        var exchanges = DbContext.Exchanges
            .Include("User")
            .Include("SourceGroup")
            .Include("SourceGroup.Students")
            .Include("TargetGroup")
            .Include("TargetGroup.Students")
            .Include("Relations")
            .Include("Relations.Exchanges")
            .Where(e => e.State == ExchangeState.Submitted && e.SourceGroup.Subject.Id == subjectId);

        ExchangeSolver.Solve(groups, exchanges);
    }

    public IEnumerable<MailGroup> GetCurrentGroups(int subjectId)
    {
        var groups = DbContext.Groups
            .Include(g => g.Students)
            .Include(g => g.Meetings)
            .Include(g => g.Subject)
            .Where(g => g.Subject.Id == subjectId && g.ClassType != "WYK").ToList();
        var studentsBySubject = groups.Select(g => new MailGroup(g.GroupNumber, g.Subject.Name.Polish,
            g.Students.Select(MailUser.FromUser)));

        return studentsBySubject;
    }

    public record MailGroup(int GroupNumber, string SubjectName, IEnumerable<MailUser> Students);

    public record MailUser(string Name, string Surname, string StudentNumber)
    {
        public static MailUser FromUser(User user) => new(user.Name, user.Surname, user.StudentNumber);
    }
}