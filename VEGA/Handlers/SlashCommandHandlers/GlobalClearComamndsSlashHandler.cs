using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class GlobalClearCommandsSlashHandler : ISlashCommandHandler
{
    public string Name => "globalclearcommands";
    public string Description => "Reset all registered commands across all guilds.";

    public async Task Execute(ApplicationCommandContext context, Vega vega)
    {
        await vega.ClearAllCommandsOnDiscordAsync();
        await context.Interaction.SendResponseAsync
        (
            InteractionCallback.Message("Cleared commands globally. May take up to 5min to be visible.")
        );
    }
}