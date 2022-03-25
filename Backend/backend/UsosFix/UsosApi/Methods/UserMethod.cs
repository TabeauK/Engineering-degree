using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.Methods
{
    public class UserMethod : ApiMethod
    {
        public UserMethod() : base("services/users/user")
        {
            Parameters.Add("fields", "first_name|last_name|student_number");
        }
    }
}
