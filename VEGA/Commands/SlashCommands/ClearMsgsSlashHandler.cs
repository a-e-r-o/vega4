using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace Commands;

public class ClearMsgsSlashHandler :  ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("clear", "Deletes recent messages")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    public async Task Clear(
        [SlashCommandParameter(Name = "count", Description = "Number of messages to delete")] int count
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

        await Context.Interaction.SendFollowupMessageAsync($"Cleared {ids.Count} messages!");
    }
}