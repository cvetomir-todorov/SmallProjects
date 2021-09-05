using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InSync.Api.Infrastructure
{
    public sealed class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (byte byteValue in value)
            {
                writer.WriteNumberValue(byteValue);
            }
            writer.WriteEndArray();
        }
    }
}