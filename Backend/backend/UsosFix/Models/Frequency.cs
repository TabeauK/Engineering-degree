using System.Runtime.Serialization;

namespace UsosFix.Models
{
    public enum Frequency
    {
        [EnumMember(Value = "every_fortnight")]
        EveryFortnight = 1, 
        [EnumMember(Value = "every_fortnight_odd")]
        EveryFortnightOdd,
        [EnumMember(Value = "every_fortnight_even")]
        EveryFortnightEven, 
        [EnumMember(Value = "every_month")]
        EveryMonth,
        [EnumMember(Value = "every_week")]
        EveryWeek,
        [EnumMember(Value = "every_working_day")]
        EveryWorkingDay, 
        [EnumMember(Value = "every_weekend")]
        EveryWeekend, 
        [EnumMember(Value = "every_day")]
        EveryDay,
        [EnumMember(Value = "other")]
        Other,
        [EnumMember(Value = "once")]
        Once
    }
}