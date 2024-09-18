using System.Collections.Concurrent;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Backend.Controllers;

[ApiController]
public class AdditionalClientsController : Controller
{
    // GET
    private readonly string _baseUrl;
    
    public AdditionalClientsController(IOptions<ApiSettings> apiSettings)
    {
        _baseUrl = apiSettings.Value.BaseUrl;
    }

    [HttpGet("simulateClients/{n}")]
    public async Task<IActionResult> SimulateClients(int n)
    {
        var tasks = new Task[n];
        var apiurl = $"{_baseUrl}gridHub";
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(apiurl)
            .Build();
        await hubConnection.StartAsync();
        hubConnection.On<GridElementExt[]>("ReceiveGrid", (Action<GridElementExt[]>)(async (receivedGrid) =>
        {
            var indices = new BlockingCollection<int>(new ConcurrentQueue<int>(Enumerable.Range(0, receivedGrid.Length)));
            for (int i = 0; i < n; i++)
            {
                tasks[i] = SimulateClient(indices);
            }
            await Task.WhenAll(tasks);
        }));



        return Ok();
    }

    private async Task SimulateClient( BlockingCollection<int> indices)
    {
        var apiurl = $"{_baseUrl}gridHub";
        HubConnection hubConnection = new HubConnectionBuilder()
            .WithUrl(apiurl)
            .Build();
        try
        {
            while (indices.TryTake(out int item))
            {
                await Task.Delay(1000);
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("FlipGridElement", item);
                await hubConnection.DisposeAsync();
                hubConnection = new HubConnectionBuilder() //hacky manier om connection te omzeilen
                    .WithUrl(apiurl)
                    .Build();
            }
        }
        finally
        {
            await hubConnection.DisposeAsync();
        }
        
    }

}