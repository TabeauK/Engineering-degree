using System.Text;

namespace UsosFix.UsosApi.OAuth
{
    public static class Utilities
    {
        /// <summary>
        /// This is a different Url Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth
        /// </summary>
        /// <param name="value">The value to Url encode</param>
        /// <returns>Returns a Url encoded string</returns>
        public static string UrlEncode(this string value)
        {
            const string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

            var result = new StringBuilder();
            Encoding enc = new UTF8Encoding(false, true);
            foreach (var symbol in value)
            {
                if (unreservedChars.Contains(symbol))
                {
                    result.Append(symbol);
                }
                else
                {
                    foreach (var byt in enc.GetBytes(symbol.ToString()))
                    {
                        result.Append($"%{byt:X2}");
                    }
                }
            }
            return result.ToString();
        }
    }
}
