using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class UsernameUserHandler : IUserCommandHandler
{
    public string Name => "username";
    public string Description => "Returns the username of the message author.";
    public async Task CommandDelegate(ApplicationCommandContext context, User user, Vega vega)
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Content = user.Username
                }
            )
        );
    }
}