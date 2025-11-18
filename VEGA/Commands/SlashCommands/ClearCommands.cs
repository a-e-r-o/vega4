using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Commands;

public class ClearCommands :  ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("clearcommands", "Erase all registered commands for this bot")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    public async Task Clear(
        [SlashCommandParameter(Name = "global", Description = "If true, erase globally registerd command. Default: false (guild only)")] bool global = false
    )
    {
        if (global)
        {
            //vegaInstance.ClearAllRegisteredCommandsAsync();
        }
        else
        {
            var guidlId = Context.Interaction.Guild!.Id;
            //vegaInstance.ClearAllRegisteredCommandsAsync(guidlId);
        }

        await Context.Interaction.SendResponseAsync
        (
            InteractionCallback.Message(string.Format("Cleared commands registered {0} for this bot", global ? "globally" : "on this guild"))
        );
    }
}