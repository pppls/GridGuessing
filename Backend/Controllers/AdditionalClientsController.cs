using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace Backend.Controllers;

[ApiController]
public class AdditionalClientsController : Controller
{
    // GET
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseUrl;
    private readonly IHubContext<GridHub> _hubContext;
    
    public AdditionalClientsController(IHttpClientFactory httpClientFactory, IOptions<ApiSettings> apiSettings, IHubContext<GridHub> hubContext)
    {
        _httpClientFactory = httpClientFactory;
        _hubContext = hubContext;
        _baseUrl = apiSettings.Value.BaseUrl;
    }

    [HttpGet("simulateClients/{n}")]
    public async Task<IActionResult> SimulateClients(int n)
    {
        var tasks = new Task[n];
        var httpClient = _httpClientFactory.CreateClient();

        for (int i = 0; i < n; i++)
        {
            tasks[i] = SimulateClient();
        }

        await Task.WhenAll(tasks);
        return Ok();
    }

    private async Task SimulateClient()
    {
        var apiurl = $"{_baseUrl}gridHub";
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(apiurl)
            .Build();

        try
        {
            await hubConnection.StartAsync();
            Random r = new Random();
            while (true)
            {
                await Task.Delay(1000);
                await hubConnection.InvokeAsync("FlipGridElement", r.Next(1, 10000));
            }
        }
        finally
        {
            await hubConnection.DisposeAsync();
        }
        
    }

}