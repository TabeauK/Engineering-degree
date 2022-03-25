using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UsosFix.Models
{
    public class User
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Username { get; set; }
        [JsonPropertyName("student_number")]
        public string StudentNumber { get; set; }
        [JsonPropertyName("first_name")]
        public string Name { get; set; }
        [JsonPropertyName("last_name")]
        public string Surname { get; set; }
        public Role Role { get; set; }
        public Language PreferredLanguage { get; set; }
        public bool Visible { get; set; }
        public bool DarkMode { get; set; }
        public ICollection<Group> Groups { get; set; }
        public ICollection<Exchange> Exchanges { get; set; }
        public ICollection<Team> Teams { get; set; }
        public ICollection<Invitation> ReceivedInvitations { get; set; }
        public ICollection<ConversationParticipant> Conversations { get; set; }
        public ICollection<ChatConnection> Chats { get; set; }
        public string DisplayName => (Visible ? $"{Name} {Surname}" : Username) + $"#{Id:00000}";
    }

    public enum Language
    {
        Polish = 1,
        English
    }
}
