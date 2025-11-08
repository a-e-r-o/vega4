using Microsoft.Extensions.Configuration;
using NetCord.Services.ApplicationCommands;
using NetCord.Gateway;
using VEGA.Core.Models;
using VEGA.Core;

// Configuration
var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
IConfiguration configuration = builder.Build();

Configuration config = new Configuration();
config.BotToken = configuration.GetValue<string>("botToken") ?? throw new Exception("token not found");

Vega vega = new(config);
await vega.Initialize();
await vega.Launch();