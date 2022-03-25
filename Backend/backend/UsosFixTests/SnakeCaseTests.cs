using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using UsosFix.Utilities;

namespace UsosFixTests
{
    class SnakeCaseTests
    {
        private class TestClass
        {
            public string? LongPropertyName { get; init; }
        }

        [Test]
        public void Pascal_To_Snake()
        {
            var obj = new TestClass {LongPropertyName = "value"};

            var str = JsonSerializer.Serialize(obj,
                new JsonSerializerOptions {PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance});
            
            Assert.That(str, Does.Contain("long_property_name"));
        }

        [Test]
        public void Snake_To_Pascal()
        {
            var str = "{ \"long_property_name\" : \"value\" }";

            var obj = JsonSerializer.Deserialize<TestClass>(str,
                new JsonSerializerOptions { PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance });

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.LongPropertyName, Is.Not.Null);
        }
    }
}
