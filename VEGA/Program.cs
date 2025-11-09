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
vega.RegisterHandler(new PongSlashHandler());
vega.RegisterHandler(new UsernameUserHandler());
vega.RegisterHandler(new LengthMsgHandler());
vega.RegisterHandler(new ClearCommandsSlashHandler());

// Initi and launch
await vega.Initialize();
await vega.Launch();