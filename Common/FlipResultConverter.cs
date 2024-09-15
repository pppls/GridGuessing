using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common;

public class FlipResultConverter : JsonConverter<FlipResult>
{
    public override FlipResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            if (doc.RootElement.TryGetProperty("Value", out _))
            {
                return JsonSerializer.Deserialize<MonetaryPrizeResult>(doc.RootElement.GetRawText(), options);
            }
            else
            {
                return JsonSerializer.Deserialize<NoPrizeResult>(doc.RootElement.GetRawText(), options);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, FlipResult value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}