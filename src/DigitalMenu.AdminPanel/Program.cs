using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DigitalMenu.AdminPanel;
using DigitalMenu.AdminPanel.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// CreateDefault loads wwwroot/appsettings.json + appsettings.{Environment}.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]?.TrimEnd('/');
if (string.IsNullOrEmpty(apiBaseUrl))
{
    apiBaseUrl = builder.HostEnvironment.IsDevelopment()
        ? "https://localhost:7182"
        : "https://digitalmenu-production-72f0.up.railway.app";
}

builder.Services.AddSingleton(new ApiSettings { BaseUrl = apiBaseUrl });
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<ApiService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
