using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using UsosFix.Models;
using UsosFix.ViewModels;
using Message = UsosFix.Models.Message;

namespace UsosFix
{
    public class ChatHub : Hub
    {
        public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger)
        {
            DbContext = context;
            Logger = logger;
        }
        private ApplicationDbContext DbContext { get; }
        private ILogger<ChatHub> Logger { get; }

        public async Task Send(string content, int conversationId)
        {
            var connection = await DbContext.ChatConnections
                .Include(c => c.User)
                .SingleOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);

            var user = connection?.User;

            if (user is null)
            {
                Log.Error("Didn't find a relevant chat connection.");
                return;
            }

            Log.Debug("Sending message from user {} to conversation {}.");


            var conversation = await DbContext.Conversations
                .SingleOrDefaultAsync(c => c.Id == conversationId &&
                                           c.Participants.Any(p => p.User.Id == user.Id));

            if (conversation is null)
            {
                Log.Error("There is no conversation with id {}.", conversationId);
                return;
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

            await NotifyNewMessage(message.Id);
            Log.Debug("Send was successful.");
        }

        public async Task NotifyNewMessage(int messageId)
        {
            var message = await DbContext.Messages
                .Include(m => m.Conversation)
                .ThenInclude(c => c.Participants)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.Chats)
                .SingleOrDefaultAsync(m => m.Id == messageId);

            var connections = message.Conversation.Participants
                .SelectMany(p => p.User.Chats)
                .Select(c => c.ConnectionId)
                .ToList();
            
            await Clients.Clients(connections).SendAsync("Receive", new StandaloneMessage(message));
        }

        public override async Task OnConnectedAsync()
        {
            Log.Debug("New connection request.");
            var token = Context.GetHttpContext().Request.Query["token"];
            var dbToken = await DbContext.Tokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t => t.Token == token.ToString());
            var user = dbToken?.User;

            if (user is null)
            {
                Log.Error("Invalid user tried to connect.");
                Context.Abort();
                return;
            }

            var connection = new ChatConnection
            {
                ConnectionId = Context.ConnectionId,
                User = user
            };
            await DbContext.ChatConnections.AddAsync(connection);
            await DbContext.SaveChangesAsync();

            Log.Debug("Created new connection for user {}, with id {}.", user.Id, connection.Id);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connection = await DbContext.ChatConnections.SingleOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);
            DbContext.ChatConnections.Remove(connection);
            await DbContext.SaveChangesAsync();
            await base.OnDisconnectedAsync(exception);
            Log.Debug("Closed connection {}.", connection.Id);
        }
    }
}
