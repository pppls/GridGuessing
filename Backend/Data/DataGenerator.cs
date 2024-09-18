namespace Backend.Data;

public static class DataGenerator
{
    public static void SeedData(GridDbContext context, int numberOfElements)
    {
        context.PromotionalGames.Add(new GridPromotionalGame
        {
            Name = "NLO",
            GridElements = new List<GridElement>()
        });
        context.SaveChanges();
        
        var gameId = context.PromotionalGames.First().Id;
        for (int i = 0; i < numberOfElements; i++)
        {
            context.GridElements.Add(new GridElement
            {
                Index = i,
                Prize = null,
                HasBeenFlipped = false,
                GridPromotionalGameId = gameId,
            });
        }

        context.SaveChanges();
        var random = new Random();
        var allIndices = Enumerable.Range(0, numberOfElements).OrderBy(_ => random.Next()).ToList();
        var indices = allIndices.Take(101).ToHashSet();
        var indexWithMainPrice = indices.First();
        indices.Remove(indexWithMainPrice);
        context.GridElements.Single(element => element.Index == indexWithMainPrice).Prize =
            new MonetaryPrize()
            {
                MonetaryValue = 25000
            };

        decimal[] consolationPrizes = { 1, 10, 100, 1000 };
        var selectedGridElements = 
            context.GridElements.Where(el => indices.Contains(el.Index));
        
        foreach (var selectedGridElement in selectedGridElements)
        {
            selectedGridElement.Prize = new MonetaryPrize()
            {
                MonetaryValue = consolationPrizes[random.Next(consolationPrizes.Length)]
            };
        }

        context.SaveChanges();
    }
}