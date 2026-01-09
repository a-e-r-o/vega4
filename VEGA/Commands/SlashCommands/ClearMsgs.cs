using Exceptions;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

public class ClearMsgs :  ApplicationCommandModule<ApplicationCommandContext>
{
    public const int MSG_COUNT_MIN = 1;
    public const int MSG_COUNT_MAX = 100;
    
    [SlashCommand("clear", "Deletes recent messages")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "count",
            Description = "Number of messages to delete",
            MinValue = MSG_COUNT_MIN,
            MaxValue = MSG_COUNT_MAX
        )]
        int msgCount
    )
    {
        // Don't trust Discord on minmax values validation
        if (
            msgCount > MSG_COUNT_MAX || msgCount < MSG_COUNT_MIN
        ) throw new SlashCommandBusinessException("Invalid params");

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        try
        {
            IAsyncEnumerable<RestMessage> messages = Context.Channel.GetMessagesAsync(
                new PaginationProperties<ulong> 
                {
                    BatchSize = 100
                }
            );

            List<ulong> msgIds = new();

            await foreach (var message in messages)
            {
                msgIds.Add(message.Id);
                
                if (msgIds.Count >= msgCount)
                {
                    break;
                }
            }

            await Context.Channel.DeleteMessagesAsync(msgIds);

            await Context.Interaction.SendFollowupMessageAsync(
                new InteractionMessageProperties
                {
                    Content = $"Here you go, I deleted {msgIds.Count} messages !",
                    Flags = MessageFlags.Ephemeral
                }
            );
        }
        catch(Exception ex)
        {
            throw new SlashCommandGenericException(ex.Message, true);
        }
    }
}