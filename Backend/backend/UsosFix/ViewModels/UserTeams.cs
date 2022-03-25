using System.Collections.Generic;
using System.Linq;

namespace UsosFix.ViewModels
{
    public record UserTeams(IEnumerable<Team> InvitedTo, IEnumerable<Team> PartOf)
    {
        public UserTeams(IEnumerable<Models.Team> invitedTo, IEnumerable<Models.Team> partOf) : this(
            invitedTo.Select(t => new Team(t)),
            partOf.Select(t => new Team(t))
        )
        {
        }
    }
}