using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.Methods
{
    public class SubjectsMethod : ApiMethod
    {
        public SubjectsMethod() : base("services/courses/user")
        {
            Parameters.Add("active_terms_only", "true");
        }
    }
}
