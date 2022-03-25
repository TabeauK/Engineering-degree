using System;
using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record StandaloneMessage
    {
        public StandaloneMessage(Models.Message model)
        {
            Content = model.Content;
            SentAt = model.SentAt;
            AuthorId = model.Author?.Id;
            Type = model.Type;
            ConversationId = model.Conversation.Id;
        }

        public int ConversationId { get; }
        public LanguageString Content { get; }
        public DateTime SentAt { get; }
        public int? AuthorId { get; }
        public MessageType Type { get; }
    }
}