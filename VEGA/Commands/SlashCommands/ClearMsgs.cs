using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

public class ClearMsgs :  ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("clear", "Deletes recent messages")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "count",
            Description = "Number of messages to delete",
            MaxValue = 100,
            MinValue = 1
        )]
        int msgCount
    )
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

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
}