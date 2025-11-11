using Microsoft.Extensions.Configuration;
using VEGA.Core.Models;
using VEGA.Core;

// Configuration
var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
IConfiguration configuration = builder.Build();

Configuration config = new Configuration();
config.BotToken = configuration.GetValue<string>("botToken") ?? throw new Exception("token not found");

// Create main Vega instance
var vega = new Vega(config);

// Initi and launch
await vega.Initialize();
await vega.Launch();