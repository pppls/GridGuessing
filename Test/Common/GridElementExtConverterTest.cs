using System.Text.Json;
using Common;

namespace Test.Common;

public class GridElementExtConverterTest
{
    private readonly JsonSerializerOptions _options;

    public GridElementExtConverterTest()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new FlipResultConverter(), new GridElementExtConverter() }
        };
    }

    [Fact]
    public void Deserialize_FlippedGridElementExt_ReturnsCorrectTypeAndValues()
    {
        // Arrange
        string json = "{\"type\":3,\"invocationId\":\"1\",\"result\":{\"result\":{\"areYouTheFirstFlipper\":true,\"value\":1},\"index\":87}}";

        // Act
        var result = JsonSerializer.Deserialize<GridElementExt>(json, _options);

        // Assert
        Assert.IsType<FlippedGridElementExt>(result);
        var flippedResult = result as FlippedGridElementExt;
        Assert.NotNull(flippedResult);
        Assert.Equal(87, flippedResult.index);
        Assert.IsType<MonetaryPrizeResult>(flippedResult.result);
        var monetaryResult = flippedResult.result as MonetaryPrizeResult;
        Assert.NotNull(monetaryResult);
        Assert.True(monetaryResult.areYouTheFirstFlipper);
        Assert.Equal(1, monetaryResult.value);
    }
}