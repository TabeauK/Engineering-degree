using System.Collections.Generic;
using System.Linq;
using Google.OrTools.Graph;
using UsosFix.Models;

namespace UsosFix.ExchangeRealization.MinCostFlowAlgorithms
{
    public class GoogleOrSolver : IExchangeSolver
    {
        public void Solve(IEnumerable<Group> groups, IEnumerable<Exchange> exchanges)
        {
            GoogleOrToolsSolve(groups, exchanges);
        }

        public static void GoogleOrToolsSolve(IEnumerable<Group> groups, IEnumerable<Exchange> exchanges)
        {
            exchanges = exchanges.ToList();
            groups = groups.ToList();
            var baseCost = 2;
            var exchangeCost = 1;
            var nullCost = 0;

            //// Vertices
            var source = 0;

            // from 1 to usersCount
            var usersVertices = groups
                .SelectMany(g => g.Students.Select(s => new UserInGroup(s, g)))
                .Select((user, index) => (user, index))
                .ToDictionary(t => t.index + 1, t => t.user);
            var usersCount = usersVertices.Count;

            // from usersCount+1 to usersCount+groupsCount
            var groupsVertices = groups
                .Select((grp, index) => (grp, index))
                .ToDictionary(t => t.index + usersCount + 1, t => t.grp);
            var groupsCount = groupsVertices.Count;
            var reverseGroupsVertices = groupsVertices.ToDictionary(p => p.Value.Id, p => p.Key);

            // usersCount+groupsCount+1
            var target = groupsCount + usersCount + 1;

            using var minCostFlow = new MinCostFlow(target + 1);

            //// Supply
            minCostFlow.SetNodeSupply(source, usersCount);
            minCostFlow.SetNodeSupply(target, -usersCount);
            for (int i = 1; i < target; ++i)
            {
                minCostFlow.SetNodeSupply(i, 0);
            }

            var exchangesInAndRelation = new List<(Exchange Considered, Exchange Done)>();


            //// Edges
            var arcs = new Dictionary<int, Exchange?>();
            foreach (var user in usersVertices)
            {
                AddArcsForUser(user.Value, user.Key);
            }

            var solveStatus = minCostFlow.SolveMaxFlowWithMinCost();

            CreateNewGroups();
            
            void AddArcsForUser(UserInGroup vertex, int userIndex)
            {
                var relevantExchanges = exchanges.Where(e =>
                    e.SourceGroup.Id == vertex.Group.Id &&
                    e.User.Id == vertex.User.Id).ToList();

                if (!relevantExchanges.Any()) return;

                var currentGroupId = reverseGroupsVertices[vertex.Group.Id];
                var arc = minCostFlow.AddArcWithCapacityAndUnitCost(userIndex, currentGroupId, 1, baseCost);
                arcs.Add(arc, null);
                arc = minCostFlow.AddArcWithCapacityAndUnitCost(source, userIndex, 1, nullCost);
                arcs.Add(arc, null);
                arc = minCostFlow.AddArcWithCapacityAndUnitCost(currentGroupId, target, 1, nullCost);
                arcs.Add(arc, null);

                foreach (var relevantExchange in relevantExchanges)
                {
                    var omit = false;
                    foreach (var relation in relevantExchange.Relations)
                    {
                        var otherExchange = relation.Exchanges.Single(e => e.Id != relevantExchange.Id);
                        if (relation.RelationType == RelationType.And && otherExchange.State == ExchangeState.Rejected)
                        {
                            relevantExchange.State = ExchangeState.Rejected;
                            omit = true;
                        }
                        if (relation.RelationType == RelationType.And && otherExchange.State == ExchangeState.Accepted)
                        {
                            // need to consider the other end
                            exchangesInAndRelation.Add((relevantExchange, otherExchange));
                        }
                        if (relation.RelationType == RelationType.Xor && otherExchange.State == ExchangeState.Rejected)
                        {
                            // don't have to do anything
                        }
                        if (relation.RelationType == RelationType.Xor && otherExchange.State == ExchangeState.Accepted)
                        {
                            relevantExchange.State = ExchangeState.Rejected;
                            omit = true;
                        }
                    }

                    if (omit) continue;

                    var id = reverseGroupsVertices[relevantExchange.TargetGroup.Id];
                    arc = minCostFlow.AddArcWithCapacityAndUnitCost(userIndex, id, 1, exchangeCost);
                    arcs.Add(arc, relevantExchange);
                    arc = minCostFlow.AddArcWithCapacityAndUnitCost(id, target, 1, nullCost);
                    arcs.Add(arc, null);
                }
            }

            void CreateNewGroups()
            {
                var exchangeArcs = arcs.Where(
                    a => minCostFlow.Flow(a.Key) > 0 &&
                    minCostFlow.UnitCost(a.Key) == exchangeCost);
                foreach (var arc in exchangeArcs)
                {
                    var exchange = arc.Value!;
                    var andRelationExchange =
                        exchangesInAndRelation
                            .SingleOrDefault(t => t.Considered.Id == exchange.Id);
                    if (andRelationExchange is not (null,null))
                    {
                        MoveUser(andRelationExchange.Done!);
                    }
                    MoveUser(exchange);
                }
            }

            void MoveUser(Exchange exchange)
            {
                var user = exchange.User;
                var group = exchange.SourceGroup;
                var newGroup = exchange.TargetGroup;

                exchange.State = ExchangeState.Accepted;
                group.Students.Remove(user);
                newGroup.Students.Add(user);
            }
        }
    }
}
