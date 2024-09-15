using System.Text.Json.Serialization;

namespace Common;

[JsonConverter(typeof(GridElementExtConverter))]
public abstract record GridElementExt(int index);
public record FlippedGridElementExt(FlipResult result, int index) : GridElementExt(index);
public record UnflippedGridElementExt(int index) : GridElementExt(index);