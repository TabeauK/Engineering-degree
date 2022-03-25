using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.Models
{
    public class ChatConnection
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}
