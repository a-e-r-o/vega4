using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class GlobalClearCommandsSlashHandler : ISlashCommandHandler
{
    public string Name => "globalclearcommands";
    public string Description => "Reset all registered commands across all guilds.";

    public async Task CommandDelegate(ApplicationCommandContext context, Vega vega)
    {
        await vega.ClearAllCommandsOnDiscordAsync();
        
        await context.Interaction.SendResponseAsync(
            NetCord.Rest.InteractionCallback.Message(
                new NetCord.Rest.InteractionMessageProperties
                {
                    Content = "cleared"
                }
            )
        );
    }
}