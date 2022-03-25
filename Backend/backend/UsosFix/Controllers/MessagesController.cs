using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UsosFix.Models;
using UsosFix.ViewModels;
using Conversation = UsosFix.ViewModels.Conversation;
using ConversationParticipant = UsosFix.Models.ConversationParticipant;
using Message = UsosFix.Models.Message;

namespace UsosFix.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        public MessagesController(ApplicationDbContext context)
        {
            DbContext = context;
        }
        private ApplicationDbContext DbContext { get; }

        /// <summary>
        /// Returns a conversation with all messages after a certain date
        /// </summary>
        /// <param name="token">Token for the user asking</param>
        /// <param name="conversationId">Id of the conversation to return</param>
        /// <param name="date">The date, if null returns the last 50 messages</param>
        /// <returns>The conversation with the requested messages</returns>
        [HttpGet]
        public async Task<ActionResult<Conversation>> MessagesSince(string token, int conversationId, DateTime? date)
        {
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            if (user is null)
            {
                return BadRequest("This token is invalid");
            }

            var conversation = await DbContext.Conversations
                .Include(c => c.Messages)
                .ThenInclude(m => m.Author)
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .AsNoTracking()
                .SingleOrDefaultAsync(c => c.Id == conversationId &&
                                           c.Participants.Any(p => p.User.Id == user.Id));

            if (conversation is null)
            {
                return BadRequest("Invalid conversation ID");
            }

            conversation.Messages = date is null 
                ? conversation.Messages.OrderByDescending(m => m.SentAt).Take(50).ToList() 
                : conversation.Messages.OrderByDescending(m => m.SentAt).SkipWhile(m => m.SentAt >= date).Take(50).ToList();

            return new Conversation(conversation);
        }

        /// <summary>
        /// Returns all conversations after a certain date
        /// </summary>
        /// <param name="token">Token for the user asking</param>
        /// <param name="date">The date, if null returns the last 50 conversations</param>
        /// <returns>A list of conversations, each with a last message</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Conversation>>> ConversationsSince(string token, DateTime? date)
        {
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            if (user is null)
            {
                return BadRequest("This token is invalid");
            }

            var conversations = DbContext.Conversations
                .Include(c => c.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Take(1))
                .ThenInclude(m => m.Author)
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .AsNoTracking()
                .Where(c => c.Participants.Any(p => p.User.Id == user.Id))
                .ToList();

            return Ok((date is null
                ? conversations.OrderByDescending(c => c.Messages.First().SentAt).Take(50)
                : conversations.OrderByDescending(c => c.Messages.First().SentAt).SkipWhile(c => c.Messages.First().SentAt >= date)).Take(50)
                .Select(c => new Conversation(c)));
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="token">Token for the sending user</param>
        /// <param name="conversationId">Id of the conversation</param>
        /// <param name="content">Content of the message</param>
        /// <returns>200</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        public async Task<IActionResult> SendMessage(string token, int conversationId, string content)
        {
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            if (user is null)
            {
                return BadRequest("This token is invalid");
            }

            var conversation = await DbContext.Conversations
                .SingleOrDefaultAsync(c => c.Id == conversationId &&
                                           c.Participants.Any(p => p.User.Id == user.Id));

            if (conversation is null)
            {
                return BadRequest("Invalid conversation ID");
            }

            var message = new Message
            {
                Author = user,
                Content = new LanguageString(content),
                Conversation = conversation,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Normal
            };
            await DbContext.Messages.AddAsync(message);
            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Invites a user to a new conversation
        /// </summary>
        /// <param name="token">Token for the inviting user</param>
        /// <param name="userToInvite">Student number of the user that should be invited</param>
        /// <returns>Id of the new conversation.</returns>
        [HttpPost]
        public async Task<IActionResult> InviteToChat(string token, string userToInvite)
        {
            var user = await DbContext.Users.SingleOrDefaultAsync(u => u.StudentNumber == userToInvite);
            var id = user?.Id;

            return id is null ? BadRequest("Invalid user id.") : await InviteToChatById(token, id.Value);
        }

        /// <summary>
        /// Invites a user to a new conversation
        /// </summary>
        /// <param name="token">Token for the inviting user</param>
        /// <param name="userToInviteId">Id of the user that should be invited</param>
        /// <returns>Id of the new conversation.</returns>
        [HttpPost]
        public async Task<IActionResult> InviteToChatById(string token, int userToInviteId)
        {
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            if (user is null)
            {
                return BadRequest("This token is invalid.");
            }
            var alreadyExists = await DbContext.Conversations
                .AnyAsync(c =>
                    c.Participants.All(p => p.User.Id == user.Id || p.User.Id == userToInviteId));
            if (alreadyExists)
            {
                return BadRequest("The conversation already exists.");
            }

            var inviting = new ConversationParticipant
            {
                State = ConversationState.Author,
                User = user
            };
            var invitedUser = await DbContext.Users.SingleOrDefaultAsync(u => u.Id == userToInviteId);
            if (invitedUser is null)
            {
                return BadRequest("Invalid student id.");
            }

            if (invitedUser.Id == user.Id)
            {
                return BadRequest("We're sorry, but you can't talk to yourself.");
            }

            var invited = new ConversationParticipant
            {
                State = ConversationState.Invited,
                User = invitedUser
            };
            var conversation = new Models.Conversation
            {
                Messages = new List<Message>(),
                Participants = new List<ConversationParticipant> { invited, inviting }
            };
            var message = new Message
            {
                Author = user,
                Content = new LanguageString($"{user.DisplayName} zaprasza do chatu.",
                    $"{user.DisplayName} invites you to chat."),
                Conversation = conversation,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Invite
            };
            conversation.Messages.Add(message);
            inviting.Conversation = conversation;
            invited.Conversation = conversation;
            await DbContext.Conversations.AddAsync(conversation);
            await DbContext.SaveChangesAsync();

            return Ok(new { ConversationId = conversation.Id });
        }


        /// <summary>
        /// Accepts an invitation to chat
        /// </summary>
        /// <param name="token">Token for the accepting user</param>
        /// <param name="conversationId">Id of the conversation</param>
        /// <returns>200</returns>
        [HttpPut]
        public async Task<IActionResult> AcceptChat(string token, int conversationId)
        {
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            if (user is null)
            {
                return BadRequest("This token is invalid");
            }

            var conversation = await DbContext.Conversations
                .Include(c => c.Messages)
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .SingleOrDefaultAsync(c => c.Id == conversationId &&
                                           c.Participants.Any(p => p.User.Id == user.Id));

            if (conversation is null)
            {
                return BadRequest("Invalid conversation ID");
            }

            var participant = conversation.Participants.SingleOrDefault(p => p.User.Id == user.Id)!;
            participant.State = ConversationState.Accepted;

            var message = new Message
            {
                Author = user,
                Content = new LanguageString($"{user.DisplayName} dołączył do chatu.",
                    $"{user.DisplayName} joined the chat."),
                Conversation = conversation,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Info
            };
            conversation.Messages.Add(message);

            await DbContext.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Rejects an invitation to chat
        /// </summary>
        /// <param name="token">Id for the user</param>
        /// <param name="conversationId">Id of the conversation</param>
        /// <returns>200</returns>
        [HttpPut]
        public async Task<IActionResult> RejectChat(string token, int conversationId)
        {
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token);
            var user = dbToken?.User;
            if (user is null)
            {
                return BadRequest("This token is invalid");
            }

            var conversation = await DbContext.Conversations
                .Include(p => p.Messages)
                .Include(p => p.Participants)
                .ThenInclude(p => p.User)
                .SingleOrDefaultAsync(c => c.Id == conversationId && 
                                           c.Participants.Any(p => p.User.Id == user.Id));

            if (conversation is null)
            {
                return BadRequest("Invalid conversation ID");
            }

            var message = new Message
            {
                Author = user,
                Content = new LanguageString($"{user.DisplayName} odrzucił zaproszenie.",
                    $"{user.DisplayName} refused to join."),
                Conversation = conversation,
                SentAt = DateTime.UtcNow,
                Type = MessageType.Info
            };
            conversation.Messages.Add(message);

            var participant = conversation.Participants.SingleOrDefault(p => p.User.Id == user.Id)!;
            participant.State = ConversationState.Rejected;
            await DbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
