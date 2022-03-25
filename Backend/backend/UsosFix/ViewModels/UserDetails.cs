using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public class UserDetails
    {
        public int Id { get; }
        public string StudentNumber { get; }
        public string Name { get; }
        public string Surname { get; }
        public string Username { get; }
        public string DisplayName { get; }
        public bool Visible { get; }
        public Role Role { get; }
        public Language PreferredLanguage { get; }
        public bool DarkMode { get; }
        public UserDetails(Models.User model)
        {
            Id = model.Id;
            StudentNumber = model.StudentNumber;
            Name = model.Name;
            Surname = model.Surname;
            Username = model.Username;
            DisplayName = model.DisplayName;
            Role = model.Role;
            PreferredLanguage = model.PreferredLanguage;
            Visible = model.Visible;
            DarkMode = model.DarkMode;
        }
    }
}