using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsosFix.ViewModels;

namespace UsosFix.Models
{
    public class Message
    {
        public int Id { get; set; }
        public LanguageString Content { get; set; }
        public DateTime SentAt { get; set; }
        public MessageType Type { get; set; }
        public User? Author { get; set; }
        public Conversation Conversation { get; set; }
    }

    public class Conversation
    {
        public int Id { get; set; }
        public ICollection<ConversationParticipant> Participants { get; set; }
        public ICollection<Message> Messages { get; set; }
    }

    public class ConversationParticipant
    {
        public int Id { get; set; }
        public ConversationState State { get; set; }
        public Conversation Conversation { get; set; }
        public User User { get; set; }
    }

    public enum ConversationState
    {
        Author,
        Accepted,
        Invited,
        Rejected
    }

    public enum MessageType
    {
        Normal,
        Info,
        Invite
    }
}
