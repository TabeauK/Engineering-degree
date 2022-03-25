namespace UsosFix.UsosApi.OAuth
{
    public class AccessTokenMethod : ApiMethod
    {
        public AccessTokenMethod(string pin, Callback callback) : base("services/oauth/access_token")
        {
            Parameters.Add("oauth_verifier", pin);
            Parameters.Add("oauth_callback", CallbackStore.GetCallback(callback));
        }
    }
}
