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
try
{
	HttpClient http = new() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
	AppSettings? appSettings = await http.GetFromJsonAsync<AppSettings>("appsettings.json");
	FirebaseConfig firebaseConfig = appSettings?.Firebase ?? new FirebaseConfig();

	Console.WriteLine($"Firebase Config Loaded: ApiKey={!string.IsNullOrEmpty(firebaseConfig.ApiKey)}");

	builder.Services.AddSingleton(firebaseConfig);
	builder.Services.AddScoped<FirebaseAuthService>();
	builder.Services.AddScoped<FirebaseService>();
}
catch (Exception ex)
{
	Console.WriteLine($"Error loading Firebase config: {ex.Message}");
	// Provide empty config as fallback
	builder.Services.AddSingleton(new FirebaseConfig());
	builder.Services.AddScoped<FirebaseAuthService>();
	builder.Services.AddScoped<FirebaseService>();
}

await builder.Build().RunAsync();
