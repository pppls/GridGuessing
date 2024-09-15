using Microsoft.AspNetCore.SignalR;

namespace WebApplication1;

public class GridHub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("UpdateGridElement", user, message);
    }
}