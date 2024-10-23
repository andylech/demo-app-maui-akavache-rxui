using System.Text.Json;
using System.Text.Json.Serialization;
using MauiCacheDemo.Api.Shared.Enums;

namespace MauiCacheDemo.Api.Converters
{
    public class PetStatusConverter : JsonConverter<PetStatus>
    {
        public override bool CanConvert(Type t) => t == typeof(PetStatus);

        public override PetStatus Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return value?.ToLower() switch
            {
                "available" => PetStatus.available,
                "pending" => PetStatus.pending,
                "sold" => PetStatus.sold,
                _ => throw new Exception("Cannot unmarshal type Status")
            };
        }

        public override void Write(Utf8JsonWriter writer, PetStatus value,
            JsonSerializerOptions options)
        {
            switch (value)
            {
                case PetStatus.available:
                    JsonSerializer.Serialize(writer, "available", options);

                    return;
                case PetStatus.pending:
                    JsonSerializer.Serialize(writer, "pending", options);

                    return;
                case PetStatus.sold:
                    JsonSerializer.Serialize(writer, "sold", options);

                    return;
                default:
                    throw new Exception("Cannot marshal type Status");
            }
        }

        public static readonly PetStatusConverter Singleton = new ();
    }
}
