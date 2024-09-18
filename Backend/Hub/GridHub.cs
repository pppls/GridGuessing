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

    public async Task<GridElementExt> FlipGridElement(int index)
    {
        //Hier moet je ook checken op de juiste promotional game, laat ik achterwegen
        var selectedElement = await _dbContext.GridElements
            .Include(el => el.Prize)
            .SingleAsync(x => x.Index == index);
        var thisClientHasAlreadyFlipped = _dbContext.GridElements
            .Where(x => x.Flipper != null)
            .Select(x => x.Flipper)
            .Contains(Context.ConnectionId);
        if (!thisClientHasAlreadyFlipped)
        {
            try
            {
                var elementWasAlreadyFlipped = selectedElement.HasBeenFlipped;
                if (!elementWasAlreadyFlipped)
                {
                    selectedElement.HasBeenFlipped = true;
                    selectedElement.Flipper = Context.ConnectionId;
                    await _dbContext.SaveChangesAsync();
                }

                FlippedGridElementExt flippedElementExt = (FlippedGridElementExt)MapDataToWeb(selectedElement);

                FlipResult resultToReturn;
                FlipResult resultToOthers;
                switch (flippedElementExt.result)
                {
                    case MonetaryPrizeResult monetaryPrizeResult:
                        resultToReturn = monetaryPrizeResult with
                        {
                            areYouTheFirstFlipper = !elementWasAlreadyFlipped
                        };
                        resultToOthers = monetaryPrizeResult with
                        {
                            areYouTheFirstFlipper = false
                        };
                        break;
                    case NoPrizeResult noPrizeResult:
                        resultToReturn = noPrizeResult;
                        resultToOthers = noPrizeResult;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await Clients.Others.SendAsync("UpdateGridElement", flippedElementExt with { result = resultToOthers });
                return flippedElementExt with { result = resultToReturn };
            }
            catch (DbUpdateConcurrencyException)
            {
                return await FlipGridElement(index);
            }
        }
        return MapDataToWeb(selectedElement);
    }

    private GridElementExt[] GetGridData()
    {
        return _dbContext.GridElements.Include(x => x.Prize).Select(MapDataToWeb).ToArray();
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
                    case NonMonetaryPrize _:
                        throw new NotImplementedException("Non-monetary prizes have yet to be implemented.");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            case false:
                return new UnflippedGridElementExt(element.Index);
        }
    }
}