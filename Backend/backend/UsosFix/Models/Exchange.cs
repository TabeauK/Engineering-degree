using System.Collections.Generic;

namespace UsosFix.Models;

public class Exchange
{
    public int Id { get; set; }
    public User User { get; set; }
    public Group SourceGroup { get; set; }
    public Group TargetGroup { get; set; }
    public ICollection<Relation> Relations { get; set; }
    public ExchangeState State { get; set; }
    public Subject Subject { get; set; }
}