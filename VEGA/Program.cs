using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Core.Models;
using Core;
using Handlers;
using Microsoft.Extensions.Caching.Memory;

// Configuration
IConfiguration appSettings = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json")
                                    .Build();

Configuration configuration = new Configuration
(
    appSettings.GetValue<string>("botToken") ?? throw new Exception("token not found"),
    appSettings.GetSection("postgres").GetValue<string>("connexionString") ?? throw new Exception("postgres connexion string not found")
);

// Build DI container
var serviceProvider = new ServiceCollection()
                            // Instance de la Configuration
                            .AddSingleton(configuration)
                            // CreateMessage Handler
                            .AddSingleton(sp => new MessageCreateHandler(
                                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MessageCreateHandler>>()
                            ))
                            .AddSingleton<IMemoryCache, MemoryCache>()
                            // Vega instance
                            .AddSingleton<Vega>()
                            // AppDbContext with Configuration
                            .AddScoped<AppDbContext>()
                            .AddScoped<GuildSettingsService>()
                            // Logging
                            .AddLogging()
                            .BuildServiceProvider();
  
// Expose provider via ServiceRegistry in Core namespace for parts that are not created via DI
GlobalRegistry.SetMainServiceProvider(serviceProvider);


// Resolve Vega instance from DI
var vega = serviceProvider.GetRequiredService<Vega>();
// Init and launch
await vega.Initialize(serviceProvider.GetRequiredService<MessageCreateHandler>(), configuration.BotToken);
await vega.Launch();