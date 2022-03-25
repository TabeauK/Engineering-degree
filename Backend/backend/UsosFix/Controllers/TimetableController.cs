using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.Services;
using UsosFix.UsosApi;
using UsosFix.ViewModels;
using GroupMeeting = UsosFix.ViewModels.GroupMeeting;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class TimetableController : ControllerBase
    {
        public TimetableController(ApplicationDbContext context, ApiConnector apiConnector,
            IHttpClientFactory httpClientFactory, ISemesterService semesterService)
        {
            DbContext = context;
            ApiConnector = apiConnector;
            HttpClientFactory = httpClientFactory;
            SemesterService = semesterService;
        }

        private ApiConnector ApiConnector { get; }
        private IHttpClientFactory HttpClientFactory { get; }
        private ApplicationDbContext DbContext { get; }
        private ISemesterService SemesterService { get; }

        /// <summary>
        ///     Get the details for a certain group of a user
        /// </summary>
        /// <param name="token">Token associated with the user</param>
        /// <param name="groupId">The id of the group</param>
        /// <returns>200 with group's details on success</returns>
        [HttpGet]
        public async Task<ActionResult<GroupDetails>> GroupInfo(string token, int groupId)
        {
            var dbToken = await DbContext.Tokens
                .Include("User")
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            var group = DbContext.Groups
                .Include("Subject")
                .Include("Subject.Semester")
                .Include("Students")
                .Include("Meetings")
                .SingleOrDefault(g => g.Id == groupId);

            if (group is null) return BadRequest("This group does not exist.");

            return new GroupDetails(group);
        }

        /// <summary>
        ///     Get the details for all groups of a user
        /// </summary>
        /// <param name="token">Token associated with the user</param>
        /// <returns>200 with groups' details on success</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDetails>>> UserGroups(string token)
        {
            var dbToken = await DbContext.Tokens
                .Include("User")
                .Include("User.Groups")
                .Include("User.Groups.Subject")
                .Include("User.Groups.Subject.Semester")
                .Include("User.Groups.Students")
                .Include("User.Groups.Meetings")
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            var groups = user.Groups.Where(g => g.Subject.Semester.IsCurrent).Select(g => new GroupDetails(g)).ToList();

            return groups;
        }

        /// <summary>
        ///     Get the details for all groups of a user, taking all exchanges into account
        /// </summary>
        /// <param name="token">Token associated with the user</param>
        /// <returns>200 with groups' details on success</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDetails>>> UserGroupsAfterExchanges(string token)
        {
            var dbToken = await DbContext.Tokens
                .Include("User")
                .Include("User.Exchanges")
                .Include("User.Exchanges.SourceGroup")
                .Include("User.Exchanges.TargetGroup")
                .Include("User.Exchanges.TargetGroup.Subject")
                .Include("User.Exchanges.TargetGroup.Students")
                .Include("User.Exchanges.TargetGroup.Meetings")
                .Include("User.Groups")
                .Include("User.Groups.Subject")
                .Include("User.Groups.Subject.Semester")
                .Include("User.Groups.Students")
                .Include("User.Groups.Meetings")
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            
            if (user is null) return Unauthorized("This token is not assigned to a user.");

            var groups = user.Groups.Where(g => g.Subject.Semester.IsCurrent);
            var exchanges = user.Exchanges;
            var joined =
                from g in groups
                join e in exchanges on g.Id equals e.SourceGroup.Id into prod
                from p in prod.DefaultIfEmpty(null)
                select p is null ? (g, State: null) : (p.TargetGroup, State: (ExchangeState?) p.State);

            return joined
                .Select(p => p.State is null 
                    ? new GroupDetails(p.TargetGroup) 
                    : new GroupDetails(p.TargetGroup, (ExchangeState)p.State)
                )
                .ToList();
        }

        /// <summary>
        ///     Get the details for all groups of a subject
        /// </summary>
        /// <param name="token">Token associated with the user</param>
        /// <param name="subjectId">Id of the subject to fetch</param>
        /// <returns>200 with groups' details on success</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDetails>>> SubjectGroups(string token, int subjectId)
        {
            var dbToken = await DbContext.Tokens
                .Include("User")
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            var subject = DbContext.Subjects.SingleOrDefault(s => s.Id == subjectId);
            
            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (subject is null) return BadRequest("Invalid SubjectId.");

            var groups = DbContext.Groups
                .Include("Subject")
                .Include("Subject.Semester")
                .Include("Students")
                .Include("Meetings")
                .Where(g => g.Subject.Id == subjectId)
                .Select(g => new GroupDetails(g))
                .ToList();

            return groups;
        }

        /// <summary>
        ///     Get the meetings for one of the groups
        /// </summary>
        /// <param name="token">Token associated with the user</param>
        /// <param name="groupId">Id of the group</param>
        /// <returns>200 with groups' details on success</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupMeeting>>> GroupMeetings(string token, int groupId)
        {
            var dbToken = await DbContext.Tokens
                .Include("User")
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            
            if (user is null) return Unauthorized("This token is not assigned to a user.");

            var group = DbContext.Groups
                .Include("Meetings")
                .Include("Meetings.Building")
                .SingleOrDefault(g => g.Id == groupId);

            if (group is null) return BadRequest("Invalid GroupId.");

            var meetings = group.Meetings.Select(m => new GroupMeeting(m)).ToList();

            return meetings;
        }
    }
}