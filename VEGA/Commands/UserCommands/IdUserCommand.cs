using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace UserCommands;

public class IdModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [UserCommand("ID")]
    public async Task Execute(User user){
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Content = $"This user's ID is : `{user.Id}`",
                    Flags = MessageFlags.Ephemeral
                }
            )
        );
    }
}