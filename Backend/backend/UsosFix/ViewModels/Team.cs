using System.Collections.Generic;
using System.Linq;

namespace UsosFix.ViewModels
{
    public record Team(int Id, SubjectOverview Subject, IEnumerable<UserOverview> Users, IEnumerable<Invitation> Invitations)
    {
        public Team(Models.Team model) : this(
            model.Id,
            new SubjectOverview(model.Subject), 
            model.Users.Select(u => new UserOverview(u)), 
            model.Invitations.Select(i => new Invitation(i))
            ) { }
    }
}
