using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using mymfm;
using mymfm.Services;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

// Configure Firebase
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var firebaseConfig = await http.GetFromJsonAsync<FirebaseConfig>("appsettings.json");
builder.Services.AddSingleton(firebaseConfig ?? new FirebaseConfig());
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirebaseService>();

await builder.Build().RunAsync();
