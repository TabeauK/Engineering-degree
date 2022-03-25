using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsosFix.UsosApi.OAuth
{
    public class RevokeTokenMethod : ApiMethod
    {
        public RevokeTokenMethod() : base("services/oauth/revoke_token")
        {
        }
    }
}
