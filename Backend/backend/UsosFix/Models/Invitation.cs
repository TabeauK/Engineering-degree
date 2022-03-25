using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public User Inviting { get; set; }
        public User Invited { get; set; }
        public Subject Subject { get; set; }
        public Team Team { get; set; }
    }
}
