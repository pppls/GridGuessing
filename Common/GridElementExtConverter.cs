namespace Common;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class GridElementExtConverter : JsonConverter<GridElementExt>
{
    public override GridElementExt Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;
            
            if (root.TryGetProperty("result", out _))
            {
                return JsonSerializer.Deserialize<FlippedGridElementExt>(root.GetRawText(), options);
            }
            else
            {
                return JsonSerializer.Deserialize<UnflippedGridElementExt>(root.GetRawText(), options);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, GridElementExt value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}