using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsosFix.Services;
using UsosFix.UsosApi;
using UsosFix.UsosApi.Methods;
using UsosFix.UsosApi.OAuth;
using UsosFix.ViewModels;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class UsosAuthorizationController : ControllerBase
    {
        public UsosAuthorizationController(ApplicationDbContext context, ApiConnector apiConnector,
            IHttpClientFactory httpClientFactory, DataLoader dataLoader)
        {
            DbContext = context;
            ApiConnector = apiConnector;
            HttpClientFactory = httpClientFactory;
            DataLoader = dataLoader;
        }

        private ApiConnector ApiConnector { get; }
        private IHttpClientFactory HttpClientFactory { get; }
        private ApplicationDbContext DbContext { get; }
        private DataLoader DataLoader { get; }

        /// <summary>
        ///     Get an unauthorized OAuth token from USOS API
        /// </summary>
        /// <param name="env">The callback to be configured on the token</param>
        /// <returns>The new token</returns>
        [HttpGet]
        public async Task<ActionResult<AccessToken>> OauthToken(string env)
        {
            var parsed = Enum.TryParse<Callback>(env, out var callback);

            if (!parsed)
                return BadRequest($"Invalid value for parameter {nameof(env)}");

            var url = ApiConnector.GetUrl(new RequestTokenMethod(callback),
                "", "", true);

            var httpClient = HttpClientFactory.CreateClient();

            try
            {
                var result = await httpClient.GetStringAsync(url);

                var p = QueryParameter.ParseQueryParameters(result);

                var token = p.Single(x => x.Name == "oauth_token").Value;
                var secret = p.Single(x => x.Name == "oauth_token_secret").Value;

                var tokenEntity = new Models.AccessToken
                {
                    Token = token, Secret = secret, Callback = callback, ValidFrom = DateTime.UtcNow, Authorized = false
                };

                await DbContext.Tokens.AddAsync(tokenEntity);
                await DbContext.SaveChangesAsync();

                return new AccessToken(tokenEntity);
            }
            catch (HttpRequestException e)
            {
                return BadRequest($"USOS API call resulted in {e.Message}");
            }
        }
        
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public async Task<ActionResult> TestOauthFlow()
        {
            var tokenResponse = await OauthToken("Local");

            var token = tokenResponse.Value;

            return Redirect($"https://apps.usos.pw.edu.pl/apps/authorize?oauth_token={token.Token}&interactivity=minimal");
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [SuppressMessage("ReSharper", "InconsistentNaming",
            Justification = "Those names are required to properly process the USOS API callback")]
        public Task<ActionResult<AccessToken>> AuthorizeTokenCallback(string oauth_verifier, string oauth_token)
            => AccessToken(oauth_verifier, oauth_token);

        /// <summary>
        /// Returns an authorized OAuth token, when given a pin and an unauthorized token.
        /// </summary>
        /// <param name="pin">The pin that the user received</param>
        /// <param name="token">The previous, unauthorized token</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<AccessToken>> AccessToken(string pin, string token)
        {
            var oldToken = DbContext.Tokens.SingleOrDefault(t => t.Token == token);

            if (oldToken is null)
            {
                return BadRequest("This token is not valid.");
            }
            
            var url = ApiConnector.GetUrl(new AccessTokenMethod(pin, oldToken.Callback),
                oldToken.Token, oldToken.Secret, true);

            var httpClient = HttpClientFactory.CreateClient();

            await using var transaction = await DbContext.Database.BeginTransactionAsync();

            try
            {
                var result = await httpClient.GetStringAsync(url);
                var p = QueryParameter.ParseQueryParameters(result);

                var t = p.Single(x => x.Name == "oauth_token").Value;
                var s = p.Single(x => x.Name == "oauth_token_secret").Value;


                var user = await DataLoader.GetAndPopulateUserForToken(t, s);

                if (user is null)
                {
                    return BadRequest();
                }

                var accessToken = new Models.AccessToken
                {
                    Token = t, Secret = s, Callback = oldToken.Callback, ValidFrom = DateTime.UtcNow, Authorized = true,
                    User = user
                };
                DbContext.Tokens.Remove(oldToken);

                DbContext.Tokens.RemoveRange(DbContext.Tokens.AsEnumerable().Where(x => x.ValidTo < DateTime.UtcNow)
                    .ToList());

                var oldAccessToken = DbContext.Tokens.SingleOrDefault(x => x.Token == accessToken.Token);
                if (oldAccessToken is null)
                {
                    await DbContext.Tokens.AddAsync(accessToken);
                }
                else
                {
                    accessToken = oldAccessToken;
                }

                await DbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AccessToken(accessToken);
            }
            catch (HttpRequestException e)
            {
                return BadRequest($"USOS API call resulted in: {e.Message}");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
        /// <summary>
        /// Checks whether the given token can be used for requests by sending a simple request to the USOS API.
        /// </summary>
        /// <param name="token">The token that should be checked</param>
        /// <returns>Whether the token is valid</returns>
        [HttpGet]
        public async Task<ActionResult<bool>> IsTokenValid(string token)
        {
            var dbToken = DbContext.Tokens.SingleOrDefault(t => t.Token == token);

            if (dbToken is null || !dbToken.Authorized) return false;

            var url = ApiConnector.GetUrl(new UserMethod(),
                dbToken.Token, dbToken.Secret, true);

            var httpClient = HttpClientFactory.CreateClient();

            try
            {
                var _ = await httpClient.GetStringAsync(url);

                return dbToken.Valid;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Logs off the user associated with the given token.
        /// </summary>
        /// <param name="token">The OAuth token</param>
        /// <returns>200</returns>
        [HttpDelete]
        public async Task<IActionResult> LogOff(string token)
        {
            var dbToken = DbContext.Tokens.SingleOrDefault(t => t.Token == token);

            if (dbToken is null)
            {
                return Ok();
            }

            var url = ApiConnector.GetUrl(new RevokeTokenMethod(), dbToken.Token, dbToken.Secret, true);
            var client = HttpClientFactory.CreateClient();
            await client.GetStringAsync(url);
            
            DbContext.Tokens.Remove(dbToken);
            await DbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
