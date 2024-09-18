using Backend.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Test;

public class DataGeneratorTest
{
    public const decimal MaxPrize = 25000;
    

    [Fact]
    public void IsThereA25000PrizeTest()
    {
        //Assert
        var dbContext = TestHelper.Setup("Data Source=:memory:;Mode=Memory;");
        var elementWithGreatestPrize = dbContext.GridElements.AsEnumerable().MaxBy(x => (x.Prize is MonetaryPrize) ? ((MonetaryPrize)x.Prize).MonetaryValue : -1);
        ((MonetaryPrize)elementWithGreatestPrize.Prize).MonetaryValue.Should().Be(MaxPrize);
    }
    
    [Fact]
    public void AreThere100ConsolationPrizesTest()
    {
        //Assert
        var dbContext = TestHelper.Setup("Data Source=:memory:;Mode=Memory;");
        dbContext.GridElements
            .AsEnumerable()
            .Count(x => (x.Prize is MonetaryPrize) && ((MonetaryPrize)x.Prize).MonetaryValue < MaxPrize).Should().Be(100);
    }
}