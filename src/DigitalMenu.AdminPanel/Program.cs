using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DigitalMenu.AdminPanel;
using DigitalMenu.AdminPanel.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var configHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var clientSettings = await configHttp.GetFromJsonAsync<ClientAppSettings>("appsettings.json")
    ?? new ClientAppSettings();
var apiBaseUrl = (clientSettings.ApiBaseUrl ?? "https://localhost:7182").TrimEnd('/');
builder.Services.AddSingleton(new ApiSettings { BaseUrl = apiBaseUrl });

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ApiService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
