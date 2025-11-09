using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class PongSlashHandler : ISlashCommandHandler
{
    public string Name => "ping";
    public string Description => "Should responds with Pong!";

    public Task<string> CommandDelegate(Vega vega)
    {
       return Task.FromResult("Pong!");
    }
}