using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp1;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var uriString = builder.Configuration["ApiBaseUrl"];

builder.Services.AddScoped(_ =>
{
    return new HttpClient { BaseAddress = new Uri(uriString!) };
});

await builder.Build().RunAsync();