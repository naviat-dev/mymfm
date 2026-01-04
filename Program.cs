using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using mymfm;
using mymfm.Services;
using System.Net.Http.Json;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Configure Firebase
HttpClient http = new() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
AppSettings? appSettings = await http.GetFromJsonAsync<AppSettings>("appsettings.json");
FirebaseConfig firebaseConfig = appSettings?.Firebase ?? new FirebaseConfig();
builder.Services.AddSingleton(firebaseConfig);
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirebaseService>();

await builder.Build().RunAsync();
