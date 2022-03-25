namespace UsosFix.UsosApi.OAuth
{
    public class RequestTokenMethod : ApiMethod
    {
        public RequestTokenMethod(Callback callback) : base("services/oauth/request_token")
        {
            Parameters.Add("oauth_callback", CallbackStore.GetCallback(callback));
            Parameters.Add("scopes", "studies");
        }
    }
}
