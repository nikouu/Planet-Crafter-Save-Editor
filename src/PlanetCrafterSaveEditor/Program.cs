using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PlanetCrafterSaveEditor;
using PlanetCrafterSaveEditor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<SaveSession>();

using (var bootHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
{
    var hashes = await bootHttp.GetFromJsonAsync<Dictionary<string, long>>("planet-hashes.json")
        ?? new Dictionary<string, long>();
    builder.Services.AddSingleton(new SpawnResolver(hashes));

    var rawCats = await bootHttp.GetFromJsonAsync<Dictionary<string, string[]>>("gid-categories.json")
        ?? new Dictionary<string, string[]>();
    var patterns = new Dictionary<WorldObjectCategory, IReadOnlyList<string>>();
    foreach (var (key, list) in rawCats)
    {
        if (Enum.TryParse<WorldObjectCategory>(key, out var cat))
        {
            patterns[cat] = list;
        }
    }
    builder.Services.AddSingleton(new WorldObjectCategorizer(patterns));

    var overrides = await bootHttp.GetFromJsonAsync<Dictionary<string, string>>("gid-overrides.json")
        ?? new Dictionary<string, string>();
    builder.Services.AddSingleton(new GIdNamer(overrides));
}

await builder.Build().RunAsync();
