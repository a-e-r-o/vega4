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
        int count
    )
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        IAsyncEnumerable<RestMessage> messages = Context.Channel.GetMessagesAsync();
        List<ulong> ids = new();

        await foreach (var message in messages)
        {
            ids.Add(message.Id);
        }

        Console.WriteLine($"Deleting {ids.Count} messages in channel {Context.Channel.Id}");

        //await Context.Client.Rest.DeleteMessagesAsync(Context.Channel.Id, ids.ToArray());

        await Context.Interaction.SendFollowupMessageAsync(
            new InteractionMessageProperties
            {
                Content = $"Cleared {ids.Count} messages!",
                Flags = MessageFlags.Ephemeral
            }
        );
    }
}