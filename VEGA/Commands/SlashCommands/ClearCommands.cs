using Core;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

#if DEBUG

public class ClearCommands :  ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("clearcommands", "Erase all registered commands for this bot")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "global",
            Description = "If true, erase globally registerd command. Default: false (guild only)"
        )] bool global = false
    )
    {
        var vegaInstance = GlobalRegistry.MainServiceProvider.GetRequiredService<Vega>();

        if (global)
        {
            await vegaInstance.ClearAllRegisteredCommandsAsync();
        }
        else
        {
            var guidlId = Context.Interaction.Guild!.Id;
            await vegaInstance.ClearAllRegisteredCommandsAsync(guidlId);
        }

        await Context.Interaction.SendResponseAsync
        (
            InteractionCallback.Message(string.Format("Cleared commands registered {0} for this bot", global ? "globally" : "on this guild"))
        );
    }
}

#endif