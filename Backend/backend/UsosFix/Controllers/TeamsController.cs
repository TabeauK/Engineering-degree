using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.Services;
using UsosFix.ViewModels;
using Invitation = UsosFix.ViewModels.Invitation;
using Team = UsosFix.Models.Team;

namespace UsosFix.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class TeamsController : ControllerBase
{
    public TeamsController(ApplicationDbContext context, ISemesterService semesterService)
    {
        DbContext = context;
        SemesterService = semesterService;
    }

    private ApplicationDbContext DbContext { get; }
    private ISemesterService SemesterService { get; }

    /// <summary>
    ///     Invites a user to our team from a certain subject
    /// </summary>
    /// <param name="token">Token for the inviting user</param>
    /// <param name="invitedId">The ID of the user we want to invite</param>
    /// <param name="subjectId">ID of the relevant subject</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> InviteUser(string token, int invitedId, int subjectId)
    {
        var dbToken = await DbContext.Tokens
            .Include("User")
            .Include("User.Teams")
            .Include("User.Teams.Subject")
            .Include("User.Teams.Subject.Semester")
            .Include("User.Teams.Users")
            .Include("User.Teams.Invitations")
            .SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;
        var invited = await DbContext.Users
            .Include(u => u.ReceivedInvitations)
            .ThenInclude(i => i.Subject)
            .SingleOrDefaultAsync(u => u.Id == invitedId);
        var subject = await DbContext.Subjects.SingleOrDefaultAsync(s => s.Id == subjectId);

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        if (invited is null)
        {
            return BadRequest("User ID is invalid.");
        }

        if (subject is null)
        {
            return BadRequest("Subject ID is invalid.");
        }

        var alreadyInvited = invited.ReceivedInvitations.Any(i => i.Subject.Id == subjectId);

        if (alreadyInvited)
        {
            return BadRequest("User is already invited");
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync();

        var team = user.Teams.SingleOrDefault(t => t.Subject.Id == subjectId);
        var invite = new Models.Invitation
        {
            Inviting = user,
            Invited = invited,
            Subject = subject
        };
        if (team is null)
        {
            team = new Team
            {
                Subject = subject,
                Users = new List<User> { user },
                Invitations = new List<Models.Invitation> { invite }
            };
            await DbContext.Teams.AddAsync(team);
        }

        invite.Team = team;
        await DbContext.Invitations.AddAsync(invite);
        await DbContext.SaveChangesAsync();

        await transaction.CommitAsync();
        return Ok();
    }

    /// <summary>
    ///     Deletes an existing invitation
    /// </summary>
    /// <param name="token">Token for the user that is either inviting or the invited in regards to this invitation</param>
    /// <param name="invitationId">The invitation ID</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> DeleteInvitation(string token, int invitationId)
    {
        var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;
        var invitation = await DbContext.Invitations
            .Include("Team")
            .Include("Team.Users")
            .SingleOrDefaultAsync(i => i.Id == invitationId);

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        if (invitation is null)
        {
            return BadRequest("This is not a valid invitation ID.");
        }

        if (invitation.Team.Users.All(u => u.Id != user.Id))
        {
            return BadRequest("This invitation is not associated with the requesting user.");
        }

        DbContext.Invitations.Remove(invitation);

        await DbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Gets all the invitations received by the given user
    /// </summary>
    /// <param name="token">The oauth token for the user</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invitation>>> GetInvitations(string token)
    {
        var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        var invitations = DbContext.Invitations
            .Include(i => i.Subject)
            .Include(i => i.Subject.Semester)
            .Include(i => i.Inviting)
            .Include(i => i.Invited)
            .Where(i => i.Invited.Id == user.Id)
            .Select(i => new Invitation(i))
            .ToList();

        return invitations;
    }

    /// <summary>
    ///     Removes a user from the given team. If the resulting team would be empty or contain a single user and no
    ///     invitations
    /// </summary>
    /// <param name="token">Token for the user that wants to perform the action</param>
    /// <param name="teamId">Id of the team in question</param>
    /// <param name="userId">Id of the user to be deleted</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> DeleteUserFromTeam(string token, int teamId, int userId)
    {
        var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        var team = await DbContext.Teams
            .Include(t => t.Users)
            .Include(t => t.Invitations)
            .SingleOrDefaultAsync(t => t.Id == teamId && t.Users.Any(u => u.Id == user.Id));

        if (team is null)
        {
            return BadRequest("This team ID is invalid.");
        }

        if (team.Users.Count == 1 || team.Users.Count == 2 && team.Invitations.Count == 0)
        {
            DbContext.Teams.Remove(team);
        }
        else
        {
            team.Users = team.Users.Where(u => u.Id != userId).ToList();
        }

        await DbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Removes a whole team at once.
    /// </summary>
    /// <param name="token">Token for the user that wants to perform the action</param>
    /// <param name="teamId">Id of the team in question</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> DeleteTeam(string token, int teamId)
    {
        var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        var team = await DbContext.Teams
            .Include(t => t.Users)
            .SingleOrDefaultAsync(t => t.Id == teamId && t.Users.Any(u => u.Id == user.Id));

        if (team is null)
        {
            return BadRequest("This team ID is invalid.");
        }

        DbContext.Teams.Remove(team);
        await DbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Accepts an invitation and adds a user to the relevant team.
    /// </summary>
    /// <param name="token">Token for the user</param>
    /// <param name="invitationId">Id of the invitation</param>
    /// <returns></returns>
    [HttpPut]
    public async Task<IActionResult> AcceptInvitation(string token, int invitationId)
    {
        var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        var invitation = await DbContext.Invitations
            .Include(i => i.Team)
            .ThenInclude(t => t.Users)
            .SingleOrDefaultAsync(i => i.Id == invitationId && i.Invited.Id == user.Id);

        if (invitation is null)
        {
            return BadRequest("This is not a valid Invitation ID.");
        }

        var team = invitation.Team;

        team.Users.Add(user);
        DbContext.Invitations.Remove(invitation);
        await DbContext.SaveChangesAsync();

        return Ok();
    }

    /// <summary>
    ///     Returns a list of all teams the user is part of.
    /// </summary>
    /// <param name="token">Token for the user</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<UserTeams>> MyTeams(string token)
    {
        var dbToken = await DbContext.Tokens
            .Include("User")
            .Include("User.Teams")
            .Include("User.Teams.Subject")
            .Include("User.Teams.Subject.Semester")
            .Include("User.Teams.Invitations")
            .Include("User.Teams.Invitations.Invited")
            .Include("User.Teams.Invitations.Inviting")
            .Include("User.Teams.Invitations.Subject")
            .Include("User.Teams.Invitations.Subject.Semester")
            .Include("User.Teams.Users")
            .SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;

        if (user is null)
        {
            return BadRequest("This token is not assigned to a user.");
        }

        var partOf = user.Teams;
        var invitedTo = DbContext.Invitations
            .Include("Team")
            .Include("Team.Subject")
            .Include("Team.Subject.Semester")
            .Include("Team.Users")
            .Include("Team.Invitations")
            .Include("Team.Invitations.Invited")
            .Include("Team.Invitations.Inviting")
            .Include("Team.Invitations.Subject")
            .Include("Team.Invitations.Subject.Semester")
            .Where(i => i.Invited.Id == user.Id)
            .Select(i => i.Team);

        return new UserTeams(invitedTo, partOf);
    }

    /// <summary>
    ///     Returns the first 10 users matching the requested prefix with their username or exactly with their student number.
    ///     Filters the user for only those that are not in a group in the given subject
    /// </summary>
    /// <param name="token">Token for the user asking</param>
    /// <param name="prefix">The prefix to check</param>
    /// <param name="subjectId">Id for the subject</param>
    /// <returns>An overview of the users data</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserOverview>>> SubjectTeamlessUserSearch(
        string token,
        string prefix,
        int subjectId)
    {
        var dbToken = await DbContext.Tokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.Token == token);
        var user = dbToken?.User;
        if (user is null)
        {
            return BadRequest("This token is invalid");
        }

        var users = DbContext.Users
            .OrderBy(u => u.StudentNumber)
            .ThenBy(u => u.Username)
            .Where(u => u.Teams.All(t => t.Subject.Id != subjectId) && u.Groups.Any(g => g.Subject.Id == subjectId))
            .Where(u => u.Id != user.Id &&
                        (u.StudentNumber == prefix || EF.Functions.ILike(u.Username, $"{prefix}%")))
            .Take(10)
            .AsNoTracking();

        return Ok(users.Select(u => new UserOverview(u)));
    }
}