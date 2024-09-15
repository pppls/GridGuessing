using System.Text.Json.Serialization;

namespace Common;

[JsonConverter(typeof(FlipResultConverter))]
public abstract record FlipResult;

public record MonetaryPrizeResult(bool AreYouTheFirstFlipper, int Value) : FlipResult;

public record NoPrizeResult : FlipResult;