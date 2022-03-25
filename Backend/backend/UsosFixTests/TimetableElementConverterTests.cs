using System;
using System.Text.Json;
using NUnit.Framework;
using UsosFix.Utilities;
using UsosFix.ViewModels;

namespace UsosFixTests
{
    public class TimetableElementConverterTests
    {
        [Test]
        public void Read_Ok()
        {
            var json = @"{
                ""room_number"": ""ONLINE"", 
                ""group_number"": 3, 
                ""classtype_name"": {""pl"": ""\u0106wiczenia"", ""en"": ""tutorials""}, 
                ""classtype_id"": ""CWI"", 
                ""course_name"": {""pl"": ""Teoria automatow i jezykow formalnych"",""en"": ""Automata Theory and Formal Languages""}, 
                ""course_id"": ""1120-IN000-ISP-0355"", 
                ""start_time"": ""2020-11-17 11:30:00"", 
                ""end_time"": ""2020-11-17 13:00:00""
            }";

            var result = JsonSerializer.Deserialize<TimetableElement>(json,
                new JsonSerializerOptions {PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance});

            Assert.NotNull(result);
            Assert.NotNull(result!.SubjectName);
            Assert.NotNull(result.Room);
            Assert.NotNull(result.Type);
            Assert.NotNull(result.GroupId);
            Assert.NotNull(result.StartTime);
            Assert.NotNull(result.EndTime);
        }

        [Test]
        public void Write_Ok()
        {
            var elem = new TimetableElement
            {
                SubjectName = new LanguageString("ą", "a"),
                Room = "ONLINE",
                Type = "CWI",
                GroupId = 3,
                StartTime = DateTime.UtcNow.ToString(),
                EndTime = DateTime.UtcNow.AddHours(1).ToString()
            };

            var json = JsonSerializer.Serialize(elem);

            Assert.That(json, Does.Contain("SubjectName"));
            Assert.That(json, Does.Contain("Room"));
            Assert.That(json, Does.Contain("Type"));
            Assert.That(json, Does.Contain("GroupId"));
            Assert.That(json, Does.Contain("StartTime"));
            Assert.That(json, Does.Contain("EndTime"));
        }


        [Test]
        public void Read_Language()
        {
            var json = @"{
                ""pl"": ""Teoria automatow i jezykow formalnych"",
                ""en"": ""Automata Theory and Formal Languages""
            }";

            var lang = JsonSerializer.Deserialize<LanguageString>(json);

            Assert.NotNull(lang);
            Assert.NotNull(lang!.English);
            Assert.NotNull(lang.Polish);
        }

        [Test]
        public void Write_Language()
        {
            var lang = new LanguageString("aaęóeaa", "aaeoeaa");

            var json = JsonSerializer.Serialize(lang);

            Assert.That(json, Is.EqualTo("{\"en\":\"aaeoeaa\",\"pl\":\"aa\\u0119\\u00F3eaa\"}"));
        }

        [Test]
        public void LanguageStringEquality()
        {
            var lang1 = new LanguageString("aaęóeaa", "aaeoeaa");
            var lang2 = new LanguageString("aaęóeaa", "aaeoeaa");

            Assert.That(lang1 == lang2);
        }

        [Test]
        public void LanguageString_Parsing()
        {
            var expected = new LanguageString("Grafika komputerowa 1", "Computer Graphics 1");

            var str = "{ \"course_name\":{\r\n\"pl\":\"Grafika komputerowa 1\",\r\n\"en\":\"Computer Graphics 1\"\r\n} }";

            var json = JsonDocument.Parse(str)
                .RootElement
                .GetProperty("course_name").GetRawText();
            
            var result = JsonSerializer.Deserialize<LanguageString>(json);

            Assert.That(result == expected);
        }
    }
}