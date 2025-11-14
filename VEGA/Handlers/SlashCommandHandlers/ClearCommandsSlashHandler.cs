using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class ClearCommandsSlashHandler : ISlashCommandHandler
{
    public string Name => "clearcommands";
    public string Description => "Reset all registered commands on the current guild.";

    public async Task CommandDelegate(ApplicationCommandContext context, Vega vega)
    {
        await vega.ClearAllCommandsOnDiscordAsync(context.Guild?.Id);

        await context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Content = "cleared"
                }
            )
        );
    }
}