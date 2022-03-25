using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsosFix.Models;
namespace UsosFix.ViewModels
{
    public class AccessToken
    {
        public string Token { get; }
        public DateTime ValidTo { get; }
        public AccessToken(Models.AccessToken model)
        {
            Token = model.Token;
            ValidTo = model.ValidTo;
        }
    }
}
