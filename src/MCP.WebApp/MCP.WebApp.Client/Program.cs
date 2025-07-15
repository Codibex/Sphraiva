using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.IO;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();