using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class ClearCommandsSlashHandler : ISlashCommandHandler
{
    public string Name => "clearcommands";
    public string Description => "Reset all registered commands.";

    public async Task<string> CommandDelegate(ApplicationCommandContext context, Vega vega)
    {
        await vega.ClearAllCommandsOnDiscordAsync();
        return "cleared";
    }
}