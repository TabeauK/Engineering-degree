using System;
using UsosFix.UsosApi.OAuth;

namespace UsosFix.Models
{
    public class AccessToken
    {
        public Callback Callback { get; set; }
        public string Token { get; set; }
        public string Secret { get; set; }
        public int Id { get; set; }
        public bool Authorized { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo => Authorized ? ValidFrom.AddHours(2) : ValidFrom.AddMinutes(15);
        public bool Valid => DateTime.UtcNow <= ValidTo;
        public User? User { get; set; }
    }
}
