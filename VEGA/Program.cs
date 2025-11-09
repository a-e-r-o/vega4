using Microsoft.Extensions.Configuration;
using NetCord.Services.ApplicationCommands;
using NetCord.Gateway;
using VEGA.Core.Models;
using VEGA.Core;
using Microsoft.Extensions.DependencyInjection;
using Handlers;
using NetCord;

// Configuration
var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
IConfiguration configuration = builder.Build();

Configuration config = new Configuration();
config.BotToken = configuration.GetValue<string>("botToken") ?? throw new Exception("token not found");

// Create main Vega instance
var vega = new Vega(config);

// Register handlers
var handlers = new List<IHandlerBase>{
    new PongSlashHandler(),
    new UsernameUserHandler(),
    new LengthMsgHandler(),
    new ClearCommandsSlashHandler(),
    new ClearMsgsSlashHandler()
};

// Initi and launch
await vega.Initialize(handlers);
await vega.Launch();