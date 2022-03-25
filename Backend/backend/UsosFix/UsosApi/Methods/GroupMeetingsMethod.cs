using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.Methods
{
    public class GroupMeetingsMethod : ApiMethod
    {
        public GroupMeetingsMethod(int groupNumber, string subjectUsosId) : base("services/tt/classgroup_dates2")
        {
            Parameters.Add("group_number", groupNumber.ToString());
            Parameters.Add("unit_id", subjectUsosId);
            Parameters.Add("fields", "type|start_time|end_time|room_number|building_name");
        }
    }
}
