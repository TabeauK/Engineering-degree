using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.ViewModels
{
    public class Invitation
    {
        public int Id { get; }
        public LanguageString SubjectName { get; }
        public UserOverview Inviting { get; }
        public UserOverview Invited { get; }
        public Invitation(Models.Invitation model)
        {
            Id = model.Id;
            SubjectName = model.Subject.Name;
            Inviting = new UserOverview(model.Inviting);
            Invited = new UserOverview(model.Invited);
        }
    }
}
