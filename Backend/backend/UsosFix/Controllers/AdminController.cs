using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.Services;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AdminController : Controller
    {
        public AdminController(ApplicationDbContext context, IMailService mailService, ISemesterService semesterService)
        {
            DbContext = context;
            MailService = mailService;
            SemesterService = semesterService;
        }
        private ApplicationDbContext DbContext { get; }
        private IMailService MailService { get; }
        private ISemesterService SemesterService { get; }

        /// <summary>
        ///     Ends exchange window, currently a no-op
        /// </summary>
        /// <param name="token">Token for the admin performing the action</param>
        /// <returns>200 Success</returns>
        [HttpPost]
        public async Task<IActionResult> EndExchangeWindow(string token)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null)
            {
                return BadRequest("This token is not assigned to a user.");
            }

            if (user.Role != Role.Admin && user.Role != Role.Leader)
            {
                return Unauthorized("This user is not allowed to perform this action.");
            }

            return Ok();
        }

        /// <summary>
        ///     Sends an email with new groups
        /// </summary>
        /// <param name="token">Token for the admin or leader performing the action</param>
        /// <param name="subjectId">Id of the subject that should be mentioned in the email</param>
        /// <returns>200 Success</returns>
        [HttpPost]
        public async Task<IActionResult> SendEmail(string token, int subjectId)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null)
            {
                return BadRequest("This token is not assigned to a user.");
            }

            if (user.Role != Role.Admin && user.Role != Role.Leader)
            {
                return Unauthorized("This user is not allowed to perform this action.");
            }

            if (await MailService.SendAsync(subjectId))
            {
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        ///     Sends an email with new groups
        /// </summary>
        /// <param name="token">Token for the admin or leader performing the action</param>
        /// <param name="subjectIds">Ids of the subjects that should be mentioned in the email</param>
        /// <returns>200 Success</returns>
        [HttpPost]
        public async Task<IActionResult> SendBatchEmail(string token, int[] subjectIds)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null)
            {
                return BadRequest("This token is not assigned to a user.");
            }

            if (user.Role != Role.Admin && user.Role != Role.Leader)
            {
                return Unauthorized("This user is not allowed to perform this action.");
            }

            if (await MailService.SendAsync(subjectIds))
            {
                return Ok();
            }

            return BadRequest();
        }

        /// <summary>
        ///     Changes the current term to the next one
        /// </summary>
        /// <param name="token">Token for the admin performing the action</param>
        /// <returns>200 Success</returns>
        [HttpPost]
        public async Task<IActionResult> ChangeToNextTerm(string token)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null)
            {
                return BadRequest("This token is not assigned to a user.");
            }

            if (user.Role != Role.Admin && user.Role != Role.Leader)
            {
                return Unauthorized("This user is not allowed to perform this action.");
            }

            await SemesterService.MoveToNextSemesterAsync();

            return Ok();
        }

        /// <summary>
        ///     Changes the current term to the requested one
        /// </summary>
        /// <param name="token">Token for the admin performing the action</param>
        /// <param name="year">Year of the new semester.</param>
        /// <param name="season">Season of the new semester.</param>
        /// <returns>200 Success</returns>
        [HttpPost]
        public async Task<IActionResult> ChangeTerm(string token, int year, SemesterSeason season)
        {
            var dbToken = await DbContext.Tokens.Include(t => t.User).SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null)
            {
                return BadRequest("This token is not assigned to a user.");
            }

            if (user.Role != Role.Admin && user.Role != Role.Leader)
            {
                return Unauthorized("This user is not allowed to perform this action.");
            }

            await SemesterService.SetCurrentSemesterAsync(year, season);

            return Ok();
        }
    }
}
