#nullable enable
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using UsosFix.ViewModels;

namespace UsosFix.Utilities
{
    public class TimetableElementConverter : JsonConverter<TimetableElement>
    {
        public override TimetableElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            LanguageString? subjectName = null;
            string? room = null;
            string? type = null;
            string? startTime = null;
            string? endTime = null;
            int? groupId = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "room_number":
                        room = JsonSerializer.Deserialize<string>(ref reader, options);
                        break;
                    case "group_number":
                        groupId = JsonSerializer.Deserialize<int>(ref reader, options);
                        break;
                    case "classtype_id":
                        type = JsonSerializer.Deserialize<string>(ref reader, options);
                        break;
                    case "course_name":
                        subjectName = JsonSerializer.Deserialize<LanguageString>(ref reader, options);
                        break;
                    case "start_time":
                        startTime = JsonSerializer.Deserialize<string>(ref reader, options);
                        break;
                    case "end_time":
                        endTime = JsonSerializer.Deserialize<string>(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }

            }

            var checks = new[]
            {
                room is null, 
                groupId is null, 
                type is null, 
                subjectName is null, 
                startTime is null, 
                endTime is null
            };

            if (checks.Any(x => x))
            {
                throw new JsonException();
            }

            return new TimetableElement
            {
                SubjectName = subjectName!,
                Room = room!,
                Type = type!,
                GroupId = groupId!.Value,
                StartTime = startTime!,
                EndTime = endTime!
            };
        }

        public override void Write(Utf8JsonWriter writer, TimetableElement value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            writer.WritePropertyName(nameof(value.SubjectName));
            writer.WriteStartObject();
            writer.WriteString("pl", value.SubjectName.Polish);
            writer.WriteString("en", value.SubjectName.English);
            writer.WriteEndObject();

            writer.WriteString(nameof(value.Room), value.Room);
            writer.WriteString(nameof(value.Type), value.Type);
            writer.WriteNumber(nameof(value.GroupId), value.GroupId);
            writer.WriteString(nameof(value.StartTime), value.StartTime);
            writer.WriteString(nameof(value.EndTime), value.EndTime);

            writer.WriteEndObject();
        }
    }
}
