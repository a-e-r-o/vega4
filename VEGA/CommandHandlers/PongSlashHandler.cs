using System.Collections.Generic;
using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class PongSlashHandler : ISlashCommandHandler
{
    public string Name => "ping";
    public string Description => "Should responds with Pong!";
    public async Task<string> CommandDelegate(ApplicationCommandContext context, Vega vega)
    {
        return "Pong!";
    }
}