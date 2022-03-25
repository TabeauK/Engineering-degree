using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UsosFix.UsosApi.OAuth;

namespace UsosFixTests
{
    class QueryParameterTests
    {
        [Test]
        public void Parse_QuestionMark()
        {
            var str = "?aaa=bbb&ccc=ddd";
            var expected = new[]
            {
                new QueryParameter("aaa", "bbb"),
                new QueryParameter("ccc", "ddd")
            };

            var result = QueryParameter.ParseQueryParameters(str);

            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void Parse_Empty()
        {
            var str = "";
            var expected = Enumerable.Empty<QueryParameter>();

            var result = QueryParameter.ParseQueryParameters(str);

            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void Parse_NoValue()
        {
            var str = "?aaa=bbb&ccc";
            var expected = new[]
            {
                new QueryParameter("aaa", "bbb"),
                new QueryParameter("ccc", "")
            };

            var result = QueryParameter.ParseQueryParameters(str);

            Assert.That(result, Is.EquivalentTo(expected));
        }
    }
}
