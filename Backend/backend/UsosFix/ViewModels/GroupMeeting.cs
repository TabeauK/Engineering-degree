using System;
using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record GroupMeeting(DateTime StartTime, DateTime EndTime, string Room, LanguageString Building)
    {
        public GroupMeeting(Models.GroupMeeting model) : this(
            model.StartTime,
            model.EndTime,
            model.Room,
            model.Building)
        {

        }
    }
}