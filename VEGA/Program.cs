using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Core.Models;
using Core;

// Configuration
IConfiguration configuration = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json")
                                    .Build();

VegaConfiguration config = new VegaConfiguration(configuration.GetValue<string>("botToken") ?? throw new Exception("token not found"));

// Build DI container
var serviceProvider = new ServiceCollection()
                            .AddSingleton(config)
                            .AddSingleton(sp => new Vega(sp.GetRequiredService<VegaConfiguration>()))
                            .AddScoped(sp => new AppDbContext(sp.GetRequiredService<VegaConfiguration>()))
                            .AddLogging()
                            .BuildServiceProvider();
  
// Expose provider via ServiceRegistry for parts that are not created via DI
ServiceRegistry.ServiceProvider = serviceProvider;

// Resolve Vega from DI (ensures ctor dependencies are injected)
var vega = serviceProvider.GetRequiredService<Vega>();

// Initi and launch
await vega.Initialize();
await vega.Launch();