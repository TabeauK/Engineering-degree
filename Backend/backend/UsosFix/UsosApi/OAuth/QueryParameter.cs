using System;
using System.Collections.Generic;

namespace UsosFix.UsosApi.OAuth
{
    public record QueryParameter(string Name, string Value)
    {
        public static IEnumerable<QueryParameter> ParseQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?"))
            {
                parameters = parameters.Remove(0, 1);
            }

            var result = new List<QueryParameter>();
            foreach (var s in parameters.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                if (s.Contains('='))
                {
                    var paramAndValue = s.Split('=');
                    result.Add(new QueryParameter(paramAndValue[0], paramAndValue[1]));
                }
                else
                {
                    result.Add(new QueryParameter(s, string.Empty));
                }
            }

            return result;
        }
    }
}
