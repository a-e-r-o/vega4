using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Commands;

public class TimestampUserCommand : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Timestamp")]
    public async Task Timestamp(RestMessage message) {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(string.Format("Length : {0}, Created at : {1}", message.Content.Length.ToString(), message.CreatedAt.ToString()))
        );
    }
}