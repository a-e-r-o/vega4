using NetCord.Services.ApplicationCommands;
using NetCord.Rest;
using VEGA.Core;

namespace Handlers;

public class LengthMsgHandler : IMessageCommandHandler
{
    public string Name => "length";
    public string Description => "Counts the number of characters in a message";

    public async Task CommandDelegate(ApplicationCommandContext context, RestMessage message, Vega vega)
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Content = "message.Content.Length.ToString()"
                }
            )
        );
    }
}