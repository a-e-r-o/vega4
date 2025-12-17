using System.Text.Json;
using System.Text.Json.Serialization;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using Models.CommandSpecificModels;
using static Core.GlobalRegistry;
using Services.CommandSpecificServices;
using Microsoft.Extensions.DependencyInjection;
using Exceptions;

namespace SlashCommands;

public class GetWaifu : ApplicationCommandModule<ApplicationCommandContext>
{
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.AttachFiles)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.AttachFiles)]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [SlashCommand("waifu", "Sends waifu images")]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "type", 
            Description = "Type of waifu to send",
            ChoicesProviderType = typeof(SfwWaifuCategoryChoicesProvider)
        )] int type = 0,
        [SlashCommandParameter(
            Name = "count", Description = "Number of waifu to send", MinValue = 1, MaxValue = 5
        )] int count = 1
    )
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        var waifuApiService = MainServiceProvider.GetRequiredService<WaifuApiService>();
        
        try
        {
            List<string> imageUrls = await waifuApiService.FetchImagesAsync(count, type);
            
            string response = string.Join("\n",imageUrls);

            await Context.Interaction.SendFollowupMessageAsync(response);
        }
        // Business exception, add info that deferred msg exists and pass down exception
        catch (SlashCommandBusinessException ex)
        {
            ex.Deferred = true;
            throw;
        }
        // API error : business exception with explicit message
        catch (HttpRequestException httpEx)
        {
            throw new SlashCommandBusinessException($"The call to the waifu API failed. Code : {httpEx.StatusCode}", true);
        }
        // Other : classic exception
        catch (Exception ex)
        {
            throw new SlashCommandGenericException(ex.Message, true);
        }
    }
}