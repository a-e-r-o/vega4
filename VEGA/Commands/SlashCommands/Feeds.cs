using Exceptions;
using Microsoft.AspNetCore.Mvc.Routing;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

public class Feeds :  ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("placehodler", "lorem ipsum")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    public async Task Execute(
        /*[SlashCommandParameter(
            Name = "userid",
            Description = "Discord user ID"
        )]
        string strUserId*/
    )
    {
        // Defer message response before any API call
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        try
        {
            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties
                {
                    Content = "lorem ipsum"
                }
            );
        }
        catch (SlashCommandException ex)
        {
            ex.Deferred = true;
            throw;
        }
    }
}