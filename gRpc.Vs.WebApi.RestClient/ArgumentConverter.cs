using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gRpc.Vs.WebApi.RestClient
{
    // source: https://github.com/dotnet/corefx/issues/36639
    public class ArgumentModelConverter : JsonConverter<ArgumentModel>
    {
        public override ArgumentModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            reader.Read();

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string propertyName = reader.GetString();

            if (!propertyName.Equals("first", StringComparison.InvariantCultureIgnoreCase))
                throw new JsonException();

            reader.Read();

            if (reader.TokenType != JsonTokenType.Number)
                throw new JsonException();

            var firstValue = reader.GetInt32();

            reader.Read();

            propertyName = reader.GetString();

            if (!propertyName.Equals("second", StringComparison.InvariantCultureIgnoreCase))
                throw new JsonException();

            reader.Read();

            var secondValue = reader.GetString();

            // end of obj
            reader.Read();

            return new ArgumentModel(firstValue, secondValue);
        }

        public override void Write(Utf8JsonWriter writer, ArgumentModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("first", value.First);
            writer.WriteString("second", value.Second);

            writer.WriteEndObject();
        }
    }
}