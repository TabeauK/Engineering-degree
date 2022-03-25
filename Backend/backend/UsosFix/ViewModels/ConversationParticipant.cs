using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record ConversationParticipant
    {
        public ConversationParticipant(Models.ConversationParticipant model)
        {
            State = model.State;
            User = new UserOverview(model.User);
        }

        public ConversationState State { get; }
        public UserOverview User { get; }
    }
}