using System.Collections.Generic;
using System.Linq;

namespace UsosFix.ViewModels
{
    public record Conversation
    {
        public Conversation(Models.Conversation model)
        {
            Id = model.Id;
            Participants = model.Participants.Select(p => new ConversationParticipant(p));
            Messages = model.Messages.Select(m => new Message(m));
        }

        public int Id { get; }
        public IEnumerable<ConversationParticipant> Participants { get; }
        public IEnumerable<Message> Messages { get; }
    }
}