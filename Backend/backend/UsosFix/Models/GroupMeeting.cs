using System;
using UsosFix.ViewModels;

namespace UsosFix.Models;

public class GroupMeeting
{
    public int Id { get; set; }
    public Group Group { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Room { get; set; }
    public LanguageString Building { get; set; }
}