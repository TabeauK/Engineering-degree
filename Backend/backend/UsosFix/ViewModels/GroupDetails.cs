using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public class GroupDetails
    {
        public int Id { get; }
        public int GroupNumber { get; }
        public SubjectOverview Subject { get; }
        public IEnumerable<UserOverview> Students { get; }
        public string Lecturers { get; }
        public string ClassType { get; }
        public int CurrentMembers { get; }
        public int MaxMembers { get; }
        public IEnumerable<GroupMeeting> Meetings { get; }
        public ExchangeState? State { get; }
        public GroupDetails(Models.Group model)
        {
            Id = model.Id;
            Subject =  new SubjectOverview(model.Subject);
            GroupNumber = model.GroupNumber;
            Students = model.Students.Select(u => new UserOverview(u));
            Lecturers = model.Lecturers;
            ClassType = model.ClassType;
            CurrentMembers = model.CurrentMembers;
            MaxMembers = model.MaxMembers;
            Meetings = model.Meetings.Select(m => new GroupMeeting(m));
        }
        public GroupDetails(Models.Group model, ExchangeState state)
        {
            Id = model.Id;
            Subject = new SubjectOverview(model.Subject);
            GroupNumber = model.GroupNumber;
            Students = model.Students.Select(u => new UserOverview(u));
            Lecturers = model.Lecturers;
            ClassType = model.ClassType;
            CurrentMembers = model.CurrentMembers;
            MaxMembers = model.MaxMembers;
            Meetings = model.Meetings.Select(m => new GroupMeeting(m));
            State = state;
        }
    }
}
