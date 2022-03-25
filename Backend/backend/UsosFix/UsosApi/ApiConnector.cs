using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;
using UsosFix.UsosApi.Methods;
using UsosFix.UsosApi.OAuth;

namespace UsosFix.UsosApi
{
    public class ApiConnector
    {
        public string BaseUrl { get; }
        private string ConsumerKey { get; }
        private string ConsumerSecret { get; }

        public ApiConnector(string baseUrl, string consumerKey, string consumerSecret)
        {
            // Base url should end with a slash.
            BaseUrl = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }

        public string GetUrl(ApiMethod method, string token,
            string tokenSecret, bool useSsl)
        {
            var url = BaseUrl + method.FullName;
            if (useSsl)
                url = url.Replace("http://", "https://");

            var parameters = string.Join("&", method.Parameters.Select(kvp => kvp.Key.UrlEncode() + "=" + kvp.Value.UrlEncode()));

            var timestamp = GenerateUnixTimeStamp();
            var nonce = GenerateNonce();
            var request = new Request(new Uri(url), parameters, ConsumerKey,
                ConsumerSecret, token, tokenSecret, "GET", timestamp, nonce);

            return $"{url}?{request.NormalizedParameters}&oauth_signature={HttpUtility.UrlEncode(request.Hash)}";
        }

        private static string GenerateUnixTimeStamp()
        {
            var timeSpan = DateTime.UtcNow - DateTime.UnixEpoch;
            return Convert.ToInt64(timeSpan.TotalSeconds).ToString();
        }

        private static string GenerateNonce() => new Random().Next(123400, 9999999).ToString();
    }
}