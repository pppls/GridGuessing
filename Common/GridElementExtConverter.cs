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
                var flippedGridElementExt = JsonSerializer.Deserialize<FlippedGridElementExt>(root, options);
                return flippedGridElementExt;
            }
            else
            {
                var rawText = root.GetRawText();
                var unflippedGridElementExt = JsonSerializer.Deserialize<UnflippedGridElementExt>(rawText, options);
                return unflippedGridElementExt;
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, GridElementExt value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}