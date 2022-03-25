using System;

namespace UsosFix.UsosApi.OAuth
{
    public abstract class CallbackStore
    {
        private static string MobileCallback { get; } = "oob";
        private static string WebCallback { get; } = "https://usosfix-ui.herokuapp.com//#/verify";
        private static string LocalCallback { get; } = "https://localhost:44304/UsosAuthorization/AuthorizeTokenCallback";
        private static string NoCallback { get; } = "oob";

        public static string GetCallback(Callback env) =>
            env switch
            {
                Callback.Web => WebCallback,
                Callback.Mobile => MobileCallback,
                Callback.None => NoCallback,
                Callback.Local => LocalCallback,
                _ => throw new ArgumentOutOfRangeException()
            };


    }

    public enum Callback { Web = 1, Mobile, None, Local }
}
