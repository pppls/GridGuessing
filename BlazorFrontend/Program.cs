using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp1;
using Common;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
var uriString = builder.Configuration["ApiBaseUrl"];

builder.Services.AddScoped(sp =>
{
    return new HttpClient { BaseAddress = new Uri(uriString) };
});

await builder.Build().RunAsync();