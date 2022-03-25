using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsosFix.Models;

namespace UsosFix.ExchangeRealization.MinCostFlowAlgorithms
{
    public interface IMinCostFlowSolver
    {
       public void CalculateMinCostFlow(IEnumerable<Group> groups, IEnumerable<Exchange> exchanges);
    }
}
