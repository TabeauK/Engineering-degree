using System.Collections.Generic;

namespace UsosFix.UsosApi
{
    public abstract class ApiMethod
    {
        protected ApiMethod(string fullName)
        {
            FullName = fullName;
        }

        public Dictionary<string, string> Parameters { get; } = new();
        public string FullName { get; } //begins with "services/"
        public string ShortName //(last element of the fully-qualified name)
        {
            get
            {
                var arr = FullName.Split('/');
                return arr[^1];
            }
        }
    }
}