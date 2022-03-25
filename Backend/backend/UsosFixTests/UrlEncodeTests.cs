using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UsosFix.UsosApi.OAuth;

namespace UsosFixTests
{
    class UrlEncodeTests
    {
        [Test]
        public void Encode_UsesUppercase()
        {
            var str = "ąęddd";

            var encoded = str.UrlEncode();
            
            Assert.That(encoded, Is.EqualTo("%C4%85%C4%99ddd"));
            Assert.That(encoded, Is.Not.EqualTo("%c4%85%c4%99ddd"));
        }
    }
}
