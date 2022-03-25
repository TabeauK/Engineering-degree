using System;
using System.Threading.Tasks;
using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record Message
    {
        public Message(Models.Message model)
        {
            Content = model.Content;
            SentAt = model.SentAt;
            AuthorId = model.Author?.Id;
            Type = model.Type;
        }
        
        public LanguageString Content { get; }
        public DateTime SentAt { get; }
        public int? AuthorId { get; }
        public MessageType Type { get; }
    }
}
