using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class ClearCommandsSlashHandler : ISlashCommandHandler
{
    public string Name => "clearcommands";
    public string Description => "Reset all registered commands on the current guild.";

    public async Task Execute(ApplicationCommandContext context, Vega vega)
    {
        if(context.Interaction.GuildId is null)
        {
            await context.Interaction.SendResponseAsync
            (
                InteractionCallback.Message("Command can only be called in guilds")
            );
        }
        else 
        {
            await vega.ClearAllCommandsOnDiscordAsync(context.Interaction.GuildId);
            await context.Interaction.SendResponseAsync
            (
                InteractionCallback.Message($"Cleared commands for this guild")
            );
        }
    }
}