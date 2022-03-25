using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UsosFix.UsosApi;
using UsosFix.UsosApi.Methods;

namespace UsosFixTests
{
    class ApiConnectorTests
    {
        [Test]
        public void GetUrl_Ok()
        {
            var baseUrl = "https://www.base.com";
            var consumerKey = "key";
            var consumerSecret = "consumer_secret";
            var token = "token";
            var tokenSecret = "token_secret";
            var connector = new ApiConnector(baseUrl, consumerKey, consumerSecret);
            var method = new UserMethod();

            var url = connector.GetUrl(method, token, tokenSecret, true);
            
            Assert.That(url, Does.Contain(baseUrl));
            Assert.That(url, Does.Contain(consumerKey));
            Assert.That(url, Does.Not.Contain(consumerSecret));
            Assert.That(url, Does.Contain(token));
            Assert.That(url, Does.Not.Contain(tokenSecret));
            Assert.That(url, Does.Contain(baseUrl));
        }

        [Test]
        public void GetUrl_ChangeProtocol()
        {
            var baseUrl = "http://www.base.com";
            var consumerKey = "key";
            var consumerSecret = "consumer_secret";
            var token = "token";
            var tokenSecret = "token_secret";
            var connector = new ApiConnector(baseUrl, consumerKey, consumerSecret);
            var method = new UserMethod();

            var url = connector.GetUrl(method, token, tokenSecret, true);

            Assert.That(url, Does.Contain("https"));
        }
    }
}
