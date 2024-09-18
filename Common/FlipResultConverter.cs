using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common;

public class FlipResultConverter : JsonConverter<FlipResult>
{
    public override FlipResult Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;
            
            if (root.TryGetProperty("areYouTheFirstFlipper", out _))
            {
                var rawText = root.GetRawText();
                var monetaryPrizeResult = JsonSerializer.Deserialize<MonetaryPrizeResult>(rawText, options);
                Console.WriteLine(monetaryPrizeResult);
                return monetaryPrizeResult;
            }
            else
            {
                return JsonSerializer.Deserialize<NoPrizeResult>(root.GetRawText(), options);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, FlipResult value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}