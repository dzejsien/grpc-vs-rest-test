using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gRpc.Vs.WebApi.Logic
{
    // source: https://github.com/dotnet/corefx/issues/36639
    public class ArgumentConverter : JsonConverter<Argument>
    {
        public override Argument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            return new Argument(firstValue, secondValue);
        }

        public override void Write(Utf8JsonWriter writer, Argument value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("first", value.First);
            writer.WriteString("second", value.Second);

            writer.WriteEndObject();
        }
    }
}