using System.Collections.Generic;
using System.Diagnostics;

namespace UsosFix.UsosApi.OAuth
{
    internal class QueryParameterComparer : IComparer<QueryParameter>
    {
        public int Compare(QueryParameter? x, QueryParameter? y)
        {
            Debug.Assert(x != null && y != null);
            if (x.Name == y.Name)
            {
                return string.CompareOrdinal(x.Value, y.Value);
            }
            return string.CompareOrdinal(x.Name, y.Name);
        }
    }
}
