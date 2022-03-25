using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.Methods
{
    public class CourseEditionMethod : ApiMethod
    {
        public CourseEditionMethod(string courseEdition) : base("services/courses/course_edition")
        {
            Parameters.Add("term_id", "2020Z");
            Parameters.Add("course_id", courseEdition);
            Parameters.Add("fields", "course_units_ids");
        }
    }
}
