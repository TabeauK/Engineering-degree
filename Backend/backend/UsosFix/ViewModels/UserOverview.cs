using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.ViewModels
{
    public record UserOverview(string DisplayName, int Id)
    {
        public UserOverview(Models.User model) : this(model.Visible ? model.DisplayName : model.Username, model.Id) { }
    }
}
