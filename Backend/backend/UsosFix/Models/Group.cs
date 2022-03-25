using System.Collections.Generic;

namespace UsosFix.Models;

public class Group
{
    public int Id { get; set; }
    public string UsosUnitId { get; set; }
    public int GroupNumber { get; set; }
    public string Lecturers { get; set; }
    public int CurrentMembers { get; set; }
    public int MaxMembers { get; set; }
    public string ClassType { get; set; }
    public Subject Subject { get; set; }
    public ICollection<User> Students { get; set; }
    public ICollection<Exchange> ExchangesTo { get; set; }
    public ICollection<Exchange> ExchangesFrom { get; set; }
    public ICollection<GroupMeeting> Meetings { get; set; }
}