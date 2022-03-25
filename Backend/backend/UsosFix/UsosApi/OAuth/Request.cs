using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UsosFix.UsosApi.OAuth
{
    public class Request
    {
#pragma warning disable IDE1006 // Naming Styles
        private const string OAuthVersion = "1.0";
        private const string OAuthParameterPrefix = "oauth_";
        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthCallbackKey = "oauth_callback";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthSignatureKey = "oauth_signature";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        private const string OAuthTokenKey = "oauth_token";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string Hmacsha1SignatureType = "HMAC-SHA1";
#pragma warning restore IDE1006 // Naming Styles

        public Request(Uri url,
            string parameters,
            string consumerKey,
            string consumerSecret,
            string token,
            string tokenSecret,
            string httpMethod,
            string timeStamp,
            string nonce)
        {
            NormalizedUrl = ConstructNormalizedUrl(url);
            NormalizedParameters = ConstructNormalizedParameters(parameters, nonce, timeStamp, Hmacsha1SignatureType, consumerKey, token);
            Hash = ConstructHash(consumerSecret, tokenSecret, httpMethod);
        }

        public string NormalizedUrl { get; }
        public string NormalizedParameters { get; }
        public string Hash { get; }

        private static string ConstructNormalizedUrl(Uri url) => $"{url.Scheme}://{url.Host}{url.AbsolutePath}";
        private static string ConstructNormalizedParameters(string parametersString, string nonce, string timeStamp, string signatureType, string consumerKey, string token)
        {
            var parameters = QueryParameter.ParseQueryParameters(parametersString)
                .Where(p => p.Name == OAuthCallbackKey || p.Name == "oauth_verifier" || !p.Name.StartsWith(OAuthParameterPrefix))
                .Concat(new[]
            {
                new QueryParameter(OAuthVersionKey, OAuthVersion),
                new QueryParameter(OAuthNonceKey, nonce),
                new QueryParameter(OAuthTimestampKey, timeStamp),
                new QueryParameter(OAuthSignatureMethodKey, signatureType),
                new QueryParameter(OAuthConsumerKeyKey, consumerKey)
            });

            if (!string.IsNullOrEmpty(token))
            {
                parameters = parameters.Append(new QueryParameter(OAuthTokenKey, token));
            }
            parameters = parameters.OrderBy(p => p, new QueryParameterComparer());
            return new StringBuilder().AppendJoin('&', parameters.Select(p => $"{p.Name}={p.Value}")).ToString();
        }
        private string ConstructHash(string consumerSecret, string tokenSecret, string httpMethod)
        {
            string signatureBase = $"{httpMethod.ToUpper()}&{NormalizedUrl.UrlEncode()}&{NormalizedParameters.UrlEncode()}";

            var hashAlgorithm = new HMACSHA1
            {
                Key = Encoding.ASCII.GetBytes(
                    $"{consumerSecret.UrlEncode()}&{tokenSecret.UrlEncode()}")
            };

            byte[] dataBuffer = Encoding.ASCII.GetBytes(signatureBase);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
