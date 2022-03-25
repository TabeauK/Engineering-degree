using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.UsosApi;
using UsosFix.UsosApi.Methods;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public AccountController(ApplicationDbContext context)
        {
            DbContext = context;
        }
        private ApplicationDbContext DbContext { get; }

        /// <summary>
        /// Sets the username for the specified user
        /// </summary>
        /// <param name="token">Token of the user that wants to change the username</param>
        /// <param name="username">The new username</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult> SetUsername(string token, [MaxLength(50)] string username)
        {
            var user = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token)?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");


            user.Username = username;

            await DbContext.SaveChangesAsync();
            
            return Ok();
        }

        /// <summary>
        /// Sets the preferred language for the user
        /// </summary>
        /// <param name="token">Token of the user</param>
        /// <param name="languageString">The preferred language, either Polish or English</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult> SetLanguage(string token, string languageString)
        {
            var parsed = Enum.TryParse<Language>(languageString, out var language);

            var user = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token)?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");
            if (!parsed) return BadRequest("This is not a valid language.");

            user.PreferredLanguage = language;
            
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Sets the preference for visibility in our application
        /// </summary>
        /// <param name="token">Token for the user</param>
        /// <param name="visible">Whether the user should be visible or not</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult> SetVisibility(string token, bool visible)
        {
            var user = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token)?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            user.Visible = visible;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Sets the preference for dark mode in our application
        /// </summary>
        /// <param name="token">Token for the user</param>
        /// <param name="darkMode">Whether the user wants to use dark mode or not</param>
        /// <returns></returns>
        [HttpPut]
        public async Task<ActionResult> SetDarkMode(string token, bool darkMode)
        {
            var user = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token)?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            user.DarkMode = darkMode;

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Returns the basic details about the user assigned to the given token. Valid only for authorized access tokens.
        /// </summary>
        /// <param name="token">The oauth token to analyze</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<ViewModels.UserDetails> WhoAmI(string token)
        {
            var dbToken = DbContext.Tokens.Include(t => t.User).SingleOrDefault(t => t.Token == token);
            var user = dbToken?.User;

            if (user is null) return Unauthorized("This token is not assigned to a user.");

            return new ViewModels.UserDetails(user);
        }

        /// <summary>
        /// Returns the first 10 users matching the requested prefix with either their student number or their username
        /// </summary>
        /// <param name="token">Token for the user asking</param>
        /// <param name="prefix">The prefix to check</param>
        /// <returns>An overview of the users data</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ViewModels.UserOverview>>> UserSearch(string token, string prefix)
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
                .Where(u => u.Id != user.Id && (u.StudentNumber == prefix || EF.Functions.ILike(u.Username, $"{prefix}%")))
                .Take(10)
                .AsNoTracking();

            return Ok(users.Select(u => new ViewModels.UserOverview(u)));
        }
    }
}
