using System.Text.Json;
using System.Text.Json.Serialization;

namespace VkActivity.Service.Helpers
{
    public class JsonBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string lowerCaseValue = value.ToLower();
            if (lowerCaseValue.Equals("true") || lowerCaseValue.Equals("yes") || lowerCaseValue.Equals("1"))
            {
                return true;
            }
            if (value.ToLower().Equals("false") || lowerCaseValue.Equals("no") || lowerCaseValue.Equals("0"))
            {
                return false;
            }

            throw new JsonException("JsonBooleanConverter.Read error");
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();

            switch (value)
            {
                case true:
                    writer.WriteStringValue("true");
                    break;
                case false:
                    writer.WriteStringValue("false");
                    break;
            }
        }
    }
}
