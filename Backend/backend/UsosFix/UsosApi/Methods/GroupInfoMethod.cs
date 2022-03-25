using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.Methods
{
    public class GroupInfoMethod : ApiMethod
    {
        public GroupInfoMethod() : base("services/groups/participant")
        {
            Parameters.Add("fields", "course_unit_id|course_id|course_name|participants|lecturers|class_type_id|group_number");
            Parameters.Add("active_terms", "true");
        }
    }
}
