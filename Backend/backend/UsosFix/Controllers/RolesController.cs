using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.ViewModels;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        public RolesController(ApplicationDbContext context) => DbContext = context;
        private ApplicationDbContext DbContext { get; }

        /// <summary>
        /// Changes the role of a user with the given student number to the one specified.
        /// </summary>
        /// <param name="token">Oauth token of an admin account</param>
        /// <param name="studentNumber">Student number for the account to change (index number)</param>
        /// <param name="roleString">One of: User, Leader, Admin</param>
        /// <returns>200 if successful, 400 otherwise</returns>
        [HttpPut]
        public async Task<IActionResult> SetRoleByStudentNumber(string token, string studentNumber, string roleString)
        {
            var dbToken = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token);
            var user = DbContext.Users.SingleOrDefault(u => u.StudentNumber == studentNumber);
            var parsed = Enum.TryParse<Role>(roleString, true, out var role);

            if (dbToken?.User is null || user is null || !parsed || dbToken.User.Role != Role.Admin)
            {
                return BadRequest("Provided parameters are invalid");
            }

            user.Role = role;
            await DbContext.SaveChangesAsync();
            
            return Ok();
        }

        /// <summary>
        /// Changes the role of a user with the given id to the one specified.
        /// </summary>
        /// <param name="token">Oauth token of an admin account</param>
        /// <param name="id">Id of the account to change</param>
        /// <param name="roleString">One of: User, Leader, Admin</param>
        /// <returns>200 if successful, 400 otherwise</returns>
        [HttpPut]
        public async Task<IActionResult> SetRoleById(string token, int id, string roleString)
        {
            var dbToken = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token);
            var user = DbContext.Users.SingleOrDefault(u => u.Id == id);
            var parsed = Enum.TryParse<Role>(roleString, true, out var role);

            if (dbToken?.User is null || user is null || !parsed || dbToken.User.Role != Role.Admin)
            {
                return BadRequest("Provided parameters are invalid");
            }

            user.Role = role;
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Returns the list of all admins and leaders.
        /// </summary>
        /// <param name="token">Oauth token of an admin account</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<UserDetails>> AdminsAndLeaders(string token)
        {
            var dbToken = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token);

            if (dbToken?.User is null || dbToken.User.Role != Role.Admin)
            {
                return BadRequest("Provided parameters are invalid");
            }

            var results = DbContext.Users.Where(u => u.Role == Role.Admin || u.Role == Role.Leader).AsNoTracking();

            return results.Select(u => new UserDetails(u)).ToList();
        }
    }
}
