using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.Methods
{
    public class UserTimetableMethod : ApiMethod
    {
        public UserTimetableMethod() : base("services/tt/user")
        {
            Parameters.Add("fields", "room_number|group_number|classtype_name|classtype_id|course_name|course_id|start_time|end_time");
        }
    }
}
