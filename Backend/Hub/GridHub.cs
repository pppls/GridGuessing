using Backend.Data;
using Common;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MonetaryPrize = Backend.Data.MonetaryPrize;

namespace Backend;

public class GridHub : Hub
{
    private GridDbContext _dbContext;

    public GridHub(GridDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var grid = GetGridData();
            if (grid == null || grid.Length == 0)
            {
                Console.WriteLine("No grid data available to send.");
            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveGrid", grid);
                Console.WriteLine("Sending grid with " + grid.Length + " elements.");
                Console.WriteLine($"First element is {grid[0]}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in OnConnectedAsync: " + ex.Message);
        }
        await base.OnConnectedAsync();
    }
    
    public async Task<FlippedGridElementExt> FlipGridElement(int index)
    {
        var flippedElement = await _dbContext.GridElements.SingleAsync(x => x.Index == index);
        var elementWasAlreadyFlipped = flippedElement.HasBeenFlipped;
        if (!elementWasAlreadyFlipped)
        {
            flippedElement.HasBeenFlipped = true;
            await _dbContext.SaveChangesAsync();
        }
        FlippedGridElementExt flippedElementExt = (FlippedGridElementExt)MapDataToWeb(flippedElement);
        FlipResult flipResult = (flippedElementExt.result) switch 
        {
            MonetaryPrizeResult prizeResult => prizeResult with { AreYouTheFirstFlipper = elementWasAlreadyFlipped },
            NoPrizeResult noPrizeResult => noPrizeResult
        };
        
        var finalFlippedElement = flippedElementExt with { result = flipResult };
        
        await Clients.Others.SendAsync("UpdateGridElement", finalFlippedElement);
        
        return finalFlippedElement;
    }
    
    private GridElementExt[] GetGridData()
    {
        return _dbContext.GridElements.Select(MapDataToWeb).ToArray();
    }

    private static GridElementExt MapDataToWeb(GridElement element)
    {
        switch (element.HasBeenFlipped)
        {
            case true:
                switch (element.Prize)
                {
                    case null:
                        return new FlippedGridElementExt(new NoPrizeResult(), element.Index);
                    case MonetaryPrize monetaryPrize:
                        return new FlippedGridElementExt(new MonetaryPrizeResult(!element.HasBeenFlipped, (int)monetaryPrize.MonetaryValue), element.Index);
                    case NonMonetaryPrize nonMonetaryPrize:
                        throw new NotImplementedException("Non-monetary prizes have yet to be implemented.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            case false:
                return new UnflippedGridElementExt(element.Index);
        }
    }
}