using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.Models
{
    public class Team
    {
        public int Id { get; set; }
        public Subject Subject { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<Invitation> Invitations { get; set; }
    }
}
