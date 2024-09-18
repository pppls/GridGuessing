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
    public void Deserialize_FlippedGridElementExtWithPrize_ReturnsCorrectTypeAndValues()
    {
        // Arrange
        string json = "{\"result\":{\"areYouTheFirstFlipper\":true,\"value\":1},\"index\":87}";

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
    
    [Fact]
    public void Deserialize_FlippedGridElementExtWithoutPrize_ReturnsCorrectTypeAndValues()
    {
        // Arrange
        string json = "{\"result\":{},\"index\":9241}";

        // Act
        var result = JsonSerializer.Deserialize<GridElementExt>(json, _options);

        // Assert
        Assert.IsType<FlippedGridElementExt>(result);
        var flippedResult = result as FlippedGridElementExt;
        Assert.NotNull(flippedResult);
        Assert.Equal(9241, flippedResult.index);
        Assert.IsType<NoPrizeResult>(flippedResult.result);
        var monetaryResult = flippedResult.result as NoPrizeResult;
        Assert.NotNull(monetaryResult);
    }
    
    [Fact]
    public void Deserialize_UnflippedGridElementExt_ReturnsCorrectTypeAndValues()
    {
        // Arrange
        string json = "{\"index\":9241}";

        // Act
        var result = JsonSerializer.Deserialize<GridElementExt>(json, _options);

        // Assert
        Assert.IsType<UnflippedGridElementExt>(result);
        var flippedResult = result as UnflippedGridElementExt;
        Assert.NotNull(flippedResult);
        Assert.Equal(9241, flippedResult.index);
    }
}