using System.Collections.Generic;
using System.Linq;
using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record Relation(int Id, IEnumerable<Exchange> Exchanges, RelationType Type)
    {
        public Relation(Models.Relation model) : this(model.Id, model.Exchanges.Select(e => new Exchange(e)), model.RelationType) { }
    }
}