using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UsosFix.Models;

namespace UsosFix.ViewModels
{
    public record LanguageString
    {
        [JsonConstructor]
        public LanguageString(string polish, string english)
        {
            Polish = polish;
            English = english;
        }

        public LanguageString(string both)
        {
            Polish = both;
            English = both;
        }

        [JsonPropertyName("en")] 
        public string English { get; private init; }

        [JsonPropertyName("pl")]
        public string Polish { get; private init; }
        
        private int SubjectId;
    }
}
