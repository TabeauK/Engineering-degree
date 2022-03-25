using System.Collections.Generic;
using UsosFix.Models;

namespace UsosFix.ExchangeRealization;

public interface IExchangeSolver
{
    public void Solve(IEnumerable<Group> groups, IEnumerable<Exchange> exchanges);
}