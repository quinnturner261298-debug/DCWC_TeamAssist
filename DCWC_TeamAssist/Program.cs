using DCWC_TeamAssist;
using DCWC_TeamAssist.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register DCWC Team Assist services
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddSingleton<CharacterDataService>();
builder.Services.AddScoped<UserCharacterService>();
builder.Services.AddScoped<TeamBuilderService>();
builder.Services.AddScoped<OcrService>();

await builder.Build().RunAsync();
