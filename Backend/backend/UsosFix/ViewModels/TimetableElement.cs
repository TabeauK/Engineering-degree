using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UsosFix.Utilities;

namespace UsosFix.ViewModels
{
    [JsonConverter(typeof(TimetableElementConverter))]
    public class TimetableElement
    {
        public LanguageString SubjectName { get; init; }
        public string Room { get; init; }
        public string Type { get; init;  }
        public int GroupId { get; init; }
        public string StartTime { get; init; }
        public string EndTime { get; init; }
    }
}