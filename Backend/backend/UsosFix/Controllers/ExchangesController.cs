using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.Services;
using UsosFix.ViewModels;
using Exchange = UsosFix.ViewModels.Exchange;
using Relation = UsosFix.ViewModels.Relation;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class ExchangesController : ControllerBase
    {
        public ExchangesController(ApplicationDbContext context, TimetableService timetableService,
            ISemesterService semesterService)
        {
            DbContext = context;
            TimetableService = timetableService;
            SemesterService = semesterService;
        }
        private ApplicationDbContext DbContext { get; }
        private TimetableService TimetableService { get; }
        private ISemesterService SemesterService { get; }

        /// <summary>
        /// Adds an exchange to a given group
        /// </summary>
        /// <param name="token">Token for the user asking for the exchange</param>
        /// <param name="groupToId">Group that the user wants to be in</param>
        /// <returns>200 if the exchange was successfully added</returns>
        [HttpPut]
        public async Task<IActionResult> AddExchange(string token, int groupToId)
        {
            var dbToken = await DbContext.Tokens
                .Include("User")
                .Include("User.Groups")
                .Include("User.Groups.Subject")
                .Include("User.Groups.Subject.Semester")
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            var groupTo = await DbContext.Groups.Include(g => g.Subject).SingleOrDefaultAsync(g => g.Id == groupToId);
            var subject = groupTo?.Subject;

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (groupTo is null || subject is null) return BadRequest("This group does not exist.");
            
            var groupFrom = user.Groups.SingleOrDefault(g => g.Subject.Id == subject.Id && g.ClassType == groupTo.ClassType);

            if (groupFrom is null || groupToId == groupFrom.Id) return BadRequest("This is not a valid exchange.");

            var exchangeAlreadyExists = DbContext.Exchanges.Any(g => g.SourceGroup.Id == groupFrom.Id && g.TargetGroup.Id == groupToId && g.User.Id == user.Id);

            if (exchangeAlreadyExists)
            {
                return Ok();
            }

            var exchange = new Models.Exchange
            {
                User = user,
                SourceGroup = groupFrom,
                TargetGroup = groupTo!,
                Subject = subject,
                Relations = new List<Models.Relation>(),
                State = ExchangeState.Submitted
            };
            await DbContext.Exchanges.AddAsync(exchange);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes an exchange
        /// </summary>
        /// <param name="token">Token for the user asking for the exchange</param>
        /// <param name="exchangeId">The id of the exchange to be deleted</param>
        /// <returns>200 if the exchange was successfully deleted</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteExchange(string token, int exchangeId)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            var exchange = await DbContext.Exchanges
                .Include(e => e.Relations)
                .SingleOrDefaultAsync(e => e.Id == exchangeId);

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (exchange is null) return BadRequest("Invalid exchangeId");
            
            DbContext.Exchanges.Remove(exchange);
            DbContext.Relations.RemoveRange(exchange.Relations);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes an exchange to the given group
        /// </summary>
        /// <param name="token">Token for the user asking for the exchange</param>
        /// <param name="groupId">The id of the target group to be deleted</param>
        /// <returns>200 if the exchange was successfully deleted</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteExchangeByGroupId(string token, int groupId)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            
            if (user is null) return Unauthorized("This token is not assigned to a user.");
            
            var exchange = await DbContext.Exchanges
                .Include(e => e.Relations)
                .SingleOrDefaultAsync(e => e.TargetGroup.Id == groupId && e.User.Id == user.Id);

            if (exchange is null) return BadRequest("Did not find a relevant exchange.");

            DbContext.Exchanges.Remove(exchange);
            DbContext.Relations.RemoveRange(exchange.Relations);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Lists all exchanges for a given user
        /// </summary>
        /// <param name="token">Token for the user in question</param>
        /// <returns>The list of all users exchanges</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exchange>>> Exchanges(string token)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            var exchanges = DbContext.Exchanges
                .Where(e => e.User.Id == user.Id)
                .Include(e => e.SourceGroup)
                .Include(e => e.SourceGroup.Subject)
                .Include(e => e.SourceGroup.Subject.Semester)
                .Include(e => e.TargetGroup)
                .Include(e => e.SourceGroup.Subject)
                .Include(e => e.TargetGroup.Subject.Semester)
                .Where(e => e.Subject.Semester.IsCurrent)
                .Select(e => new Exchange(e));

            return Ok(exchanges);
        }

        /// <summary>
        /// Stop all exchanges
        /// </summary>
        /// <param name="token">Token for an admin</param>
        /// <returns>200</returns>
        [HttpPut]
        public async Task<IActionResult> EndExchangeWindow(string token)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (user.Role != Role.Admin) return Unauthorized("This user is not authorized to perform this action.");

            await using var transaction = await DbContext.Database.BeginTransactionAsync();
            var pendingExchanges = DbContext.Exchanges.Where(e => e.State == ExchangeState.Submitted);
            foreach (var exchange in pendingExchanges)
            {
                exchange.State = ExchangeState.Rejected;
            }
            await DbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return Ok();
        }

        /// <summary>
        /// Stop and try to execute all exchanges for the given subjects
        /// </summary>
        /// <param name="token">Token for an admin or leader</param>
        /// <param name="subjectIds">The ids for subjects that should be processed</param>
        /// <returns>200</returns>
        [HttpPut]
        public async Task<IActionResult> RealizeExchangesInSubjects(string token, int[] subjectIds)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (user.Role == Role.User) return Unauthorized("This user is not authorized to perform this action.");

            foreach (var id in subjectIds)
            {
                await using var transaction = await DbContext.Database.BeginTransactionAsync();
                TimetableService.MakeExchanges(id);
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            return Ok();
        }

        /// <summary>
        /// Get a summary for all exchanges
        /// </summary>
        /// <param name="token">Token for an admin or a leader</param>
        /// <returns>The summary for all exchanges</returns>
        [HttpGet]
        public async Task<ActionResult<ExchangeSummary>> ExchangesSummary(string token)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (user.Role == Role.User) return Unauthorized("This user is not authorized to perform this action.");

            var submitted = DbContext.Exchanges.Where(e => e.Subject.Semester.IsCurrent)
                .Count(e => e.State == ExchangeState.Submitted);
            var accepted = DbContext.Exchanges.Where(e => e.Subject.Semester.IsCurrent)
                .Count(e => e.State == ExchangeState.Accepted);
            var rejected = DbContext.Exchanges.Where(e => e.Subject.Semester.IsCurrent)
                .Count(e => e.State == ExchangeState.Rejected);

            return new ExchangeSummary(submitted, accepted, rejected);
        }

        /// <summary>
        /// Get a summary for all exchanges within a given subject
        /// </summary>
        /// <param name="token">Token for an admin or a leader</param>
        /// <param name="subjectId">The id of the subject</param>
        /// <returns>The summary for all exchanges within the given subject</returns>
        [HttpGet]
        public async Task<ActionResult<ExchangeSummary>> ExchangesSummaryBySubject(string token, int subjectId)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (user.Role == Role.User) return Unauthorized("This user is not authorized to perform this action.");

            var submitted = DbContext.Exchanges.Count(e => e.SourceGroup.Subject.Id == subjectId && e.State == ExchangeState.Submitted);
            var accepted = DbContext.Exchanges.Count(e => e.SourceGroup.Subject.Id == subjectId && e.State == ExchangeState.Accepted);
            var rejected = DbContext.Exchanges.Count(e => e.SourceGroup.Subject.Id == subjectId && e.State == ExchangeState.Rejected);

            return new ExchangeSummary(submitted, accepted, rejected);
        }

        /// <summary>
        /// Adds a relation between the two given exchanges
        /// </summary>
        /// <param name="token">Token for the user asking for the exchange</param>
        /// <param name="relationType"></param>
        /// <param name="exchange1Id">Exchange one</param>
        /// <param name="exchange2Id">Exchange two</param>
        /// <returns>200 if the relation was successfully added</returns>
        [HttpPut]
        public async Task<IActionResult> AddRelation(string token, int exchange1Id, int exchange2Id, string relationType)
        {
            if (exchange1Id == exchange2Id) return BadRequest("Exchange 1 is equal to Exchange 2.");
            
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            var exchange1 = await DbContext.Exchanges.SingleOrDefaultAsync(e => e.Id == exchange1Id);
            var exchange2 = await DbContext.Exchanges.SingleOrDefaultAsync(e => e.Id == exchange2Id);
            var parsed = Enum.TryParse<RelationType>(relationType, out var type);
            
            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (!parsed) return BadRequest("This is not a valid relationType.");
            if (exchange1 is null || exchange2 is null) return BadRequest("Invalid exchanges.");
            if (exchange1.User.Id != user.Id || exchange2.User.Id != user.Id) return BadRequest("The requested exchanges do not belong to this user.");

            var relation = new Models.Relation
            {
                RelationType = type,
                Exchanges = new []{exchange1, exchange2}
            };
            await DbContext.Relations.AddAsync(relation);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Deletes a relation
        /// </summary>
        /// <param name="token">Token for the user asking for the exchange</param>
        /// <param name="relationId">The id of the relation to be deleted</param>
        /// <returns>200 if the relation was successfully deleted</returns>
        [HttpPut]
        public async Task<IActionResult> DeleteRelation(string token, int relationId)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            var relation = DbContext.Relations
                .Include(r => r.Exchanges)
                .ThenInclude(e => e.User)
                .SingleOrDefault(r => r.Id == relationId);
            var exchange = relation?.Exchanges.FirstOrDefault();
            
            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (exchange is null || relation is null) return BadRequest("This is not a valid relationId");
            if (exchange.User.Id != user.Id) return BadRequest("This relation is not related to this user.");

            DbContext.Relations.Remove(relation);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// All relations for the user
        /// </summary>
        /// <param name="token">User's Oauth token</param>
        /// <returns>List of relations</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Relation>>> Relations(string token)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            var relations = DbContext.Relations
                .Include("Exchanges")
                .Include("Exchanges.SourceGroup")
                .Include("Exchanges.SourceGroup.Subject")
                .Include("Exchanges.SourceGroup.Subject.Semester")
                .Include("Exchanges.SourceGroup")
                .Include("Exchanges.SourceGroup.Subject")
                .Include("Exchanges.TargetGroup.Subject.Semester")
                .Where(r => r.Exchanges.Any(e => e.User.Id == user.Id && e.Subject.Semester.IsCurrent))
                .AsNoTracking();

           return relations.Select(r => new Relation(r)).ToArray();
        }
    }
}
