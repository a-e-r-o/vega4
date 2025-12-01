using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Core.Models;
using Core;
using Handlers;

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
                            // Vega instance
                            .AddSingleton(sp => new Vega())
                            // AppDbContext with Configuration
                            .AddScoped(sp => new AppDbContext(
                                sp.GetRequiredService<Configuration>()
                            ))
                            // Logging
                            .AddLogging()
                            .BuildServiceProvider();
  
// Resolve Vega from DI (ensures ctor dependencies are injected)
var vega = serviceProvider.GetRequiredService<Vega>();

// Expose provider via ServiceRegistry in Core namespace for parts that are not created via DI
Core.GlobalRegistry.MainServiceProvider = serviceProvider;

// Init and launch
await vega.Initialize(serviceProvider.GetRequiredService<MessageCreateHandler>(), configuration.BotToken);
await vega.Launch();