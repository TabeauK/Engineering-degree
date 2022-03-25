using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.Models
{
    public class Relation
    {
        public int Id { get; set; }
        public ICollection<Exchange> Exchanges { get; set; }
        public RelationType RelationType { get; set; }
    }
}
