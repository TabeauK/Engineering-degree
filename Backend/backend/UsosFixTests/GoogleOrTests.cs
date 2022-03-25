using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Google.OrTools;
using Google.OrTools.Graph;
using Google.OrTools.Sat;
using UsosFix.ExchangeRealization.MinCostFlowAlgorithms;
using UsosFix.Models;

namespace UsosFixTests
{
    class GoogleOrTests
    {
        [Test]
        public void K22()
        {
            var subject = new Subject();
            var u1 = new User { Id = 1 };
            var u2 = new User { Id = 2 };
            var g1 = new Group { Id = 1, Subject = subject, Students = new List<User> { u1 }, CurrentMembers = 1, MaxMembers = 1 };
            var g2 = new Group { Id = 2, Subject = subject, Students = new List<User> { u2 }, CurrentMembers = 1, MaxMembers = 1 };
            var e1 = new Exchange { SourceGroup = g1, TargetGroup = g2, User = u1, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e2 = new Exchange { SourceGroup = g2, TargetGroup = g1, User = u2, State = ExchangeState.Submitted, Relations = new List<Relation>() };

            var exchanges = new[] { e1, e2 };
            var groups = new[] { g1, g2 };
            GoogleOrSolver.GoogleOrToolsSolve(groups, exchanges);

            Assert.That(e1.State, Is.EqualTo(ExchangeState.Accepted));
            Assert.That(e2.State, Is.EqualTo(ExchangeState.Accepted));
            Assert.That(g1.Students, Is.EquivalentTo(new[] { u2 }));
            Assert.That(g2.Students, Is.EquivalentTo(new[] { u1 }));
        }

        [MaxTime(2000)]
        [TestCase(1000)]
        [TestCase(10000)]
        public void LargeSimpleExample(int n)
        {
            var subject = new Subject();

            List<User> users = new();
            List<Group> groups = new();
            List<Exchange> exchanges = new();

            for (int i = 0; i < n; ++i)
            {
                var u = new User { Id = i };
                users.Add(u);
            }
            for (int i = 0; i < n; ++i)
            {
                var g = new Group { Id = i, Subject = subject, Students = new List<User> { users[i] }, CurrentMembers = 1, MaxMembers = 1 };
                groups.Add(g);
            }
            for (int i = 0; i < n - 1; ++i)
            {
                var e = new Exchange { Id = i, SourceGroup = groups[i], TargetGroup = groups[i + 1], User = users[i], State = ExchangeState.Submitted, Relations = new List<Relation>() };
                exchanges.Add(e);
            }
            GoogleOrSolver.GoogleOrToolsSolve(groups, exchanges);

            for (int i = 1; i < n - 1; ++i)
            {
                Assert.That(groups[i].Students, Does.Contain(users[i - 1]));
            }
        }

        private void RandomExample(int groupCount, int groupSize, int exchangeCount)
        {
            var subject = new Subject();
            var random = new Random(123123);
            List<Group> groups = new();
            List<Exchange> exchanges = new();
            for (int i = 0; i < groupCount; ++i)
            {
                List<User> users = new();
                var userStartIndex = i * groupSize;
                var userEndIndex = (i + 1) * groupSize;
                for (int j = userStartIndex; j < userEndIndex; ++j)
                {
                    var u = new User { Id = j };
                    users.Add(u);
                }

                var g = new Group { Id = i, Subject = subject, Students = users, CurrentMembers = groupSize, MaxMembers = groupSize + random.Next(5) };
                groups.Add(g);
            }
            for (int i = 0; i < exchangeCount; ++i)
            {
                var from = random.Next(groupCount);
                var to = random.Next(groupCount);
                if (from != to)
                {
                    var userStartIndex = from * groupSize;
                    var userEndIndex = (from + 1) * groupSize;
                    var group = groups[from];
                    var user = random.Next(group.Students.Count);
                    var e = new Exchange { Id = i, SourceGroup = groups[from], TargetGroup = groups[to], User = group.Students.Skip(user).First(), State = ExchangeState.Submitted, Relations = new List<Relation>() };
                    exchanges.Add(e);
                }
            }
            GoogleOrSolver.GoogleOrToolsSolve(groups, exchanges);
        }

        [MaxTime(2000)]
        [TestCase(10, 15, 1000)]
        [TestCase(10, 30, 2000)]
        [TestCase(100, 15, 10000)]
        [TestCase(100, 30, 20000)]
        [TestCase(200, 15, 20000)]
        [TestCase(200, 30, 15000)]
        public void SmallRandomExample(int groupCount, int groupSize, int exchangeCount) =>
            RandomExample(groupCount, groupSize, exchangeCount);

        [Ignore("Too long for automatic tests")]
        [MaxTime(60_000)]
        [TestCase(1000, 15, 100_000)]
        [TestCase(1000, 30, 100_000)]
        [TestCase(2000, 15, 50_000)]
        [TestCase(2000, 30, 50_000)]
        [TestCase(5000, 15, 50_000)]
        public void LargeRandomExample(int groupCount, int groupSize, int exchangeCount) =>
            RandomExample(groupCount, groupSize, exchangeCount);


        [Test]
        public void K22_MultipleMembers()
        {
            var subject = new Subject();
            var u11 = new User { Id = 1 };
            var u12 = new User { Id = 2 };
            var u21 = new User { Id = 3 };
            var u22 = new User { Id = 4 };
            var g1 = new Group { Id = 1, Subject = subject, Students = new List<User> { u11, u12 }, CurrentMembers = 2, MaxMembers = 2 };
            var g2 = new Group { Id = 2, Subject = subject, Students = new List<User> { u21, u22 }, CurrentMembers = 2, MaxMembers = 2 };
            var e1 = new Exchange { SourceGroup = g1, TargetGroup = g2, User = u11, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e2 = new Exchange { SourceGroup = g2, TargetGroup = g1, User = u22, State = ExchangeState.Submitted, Relations = new List<Relation>() };

            var exchanges = new[] { e1, e2 };
            var groups = new[] { g1, g2 };
            GoogleOrSolver.GoogleOrToolsSolve(groups, exchanges);

            Assert.That(e1.State, Is.EqualTo(ExchangeState.Accepted));
            Assert.That(e2.State, Is.EqualTo(ExchangeState.Accepted));
            Assert.That(g1.Students, Is.EquivalentTo(new[] { u12, u22 }));
            Assert.That(g2.Students, Is.EquivalentTo(new[] { u11, u21 }));
        }

        [Test]
        public void ThreeSingleUserGroups()
        {
            var subject = new Subject();
            var u1 = new User { Id = 1 };
            var u2 = new User { Id = 2 };
            var u3 = new User { Id = 3 };
            var g1 = new Group { Id = 1, Subject = subject, Students = new List<User> { u1 }, CurrentMembers = 1, MaxMembers = 2 };
            var g2 = new Group { Id = 2, Subject = subject, Students = new List<User> { u2 }, CurrentMembers = 1, MaxMembers = 2 };
            var g3 = new Group { Id = 3, Subject = subject, Students = new List<User> { u3 }, CurrentMembers = 1, MaxMembers = 2 };
            var e12 = new Exchange { SourceGroup = g1, TargetGroup = g2, User = u1, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e13 = new Exchange { SourceGroup = g1, TargetGroup = g3, User = u1, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e21 = new Exchange { SourceGroup = g2, TargetGroup = g1, User = u2, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e23 = new Exchange { SourceGroup = g2, TargetGroup = g3, User = u2, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e31 = new Exchange { SourceGroup = g3, TargetGroup = g1, User = u3, State = ExchangeState.Submitted, Relations = new List<Relation>() };
            var e32 = new Exchange { SourceGroup = g3, TargetGroup = g2, User = u3, State = ExchangeState.Submitted, Relations = new List<Relation>() };

            var exchanges = new[] { e12, e13, e21, e23, e31, e32 };
            var groups = new[] { g1, g2, g3 };
            GoogleOrSolver.GoogleOrToolsSolve(groups, exchanges);

            var exchangesStates = exchanges.Select(e => e.State);
            var expectedStates = new[]
            {
                ExchangeState.Submitted,
                ExchangeState.Submitted,
                ExchangeState.Submitted,
                ExchangeState.Accepted,
                ExchangeState.Accepted,
                ExchangeState.Accepted
            };

            Assert.That(exchangesStates, Is.EquivalentTo(expectedStates));
            Assert.That(g1.Students, Does.Not.Contain(u1));
            Assert.That(g2.Students, Does.Not.Contain(u2));
            Assert.That(g3.Students, Does.Not.Contain(u3));
        }

        [Test]
        public void RelationsSat()
        {
            var model = new Google.OrTools.Sat.CpModel();
            var solver = new Google.OrTools.Sat.CpSolver();

            var variables = new[] { 0, 1, 2, 3, 4 };
            var literals = variables.Select(x => model.NewBoolVar(x.ToString())).ToList();

            model.AddBoolAnd(new[] { literals[0], literals[1] });
            model.AddBoolAnd(new[] { literals[0], literals[4] });
            model.AddBoolXor(new[] { literals[1], literals[2] });
            model.AddBoolAnd(new[] { literals[2], literals[3] });
            model.Maximize(LinearExpr.Sum(literals));

            var modelState = model.Validate();
            
            var response = solver.Solve(model);

            var result = solver.ObjectiveValue;
        }
    }
}
