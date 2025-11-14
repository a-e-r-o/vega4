using System.Collections.Generic;
using NetCord;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class ClearMsgsSlashHandler : ISlashCommandHandler
{
    public string Name => "clear";
    public string Description => "Deletes recent messages";
    public async Task CommandDelegate(ApplicationCommandContext context, Vega vega)
    {
        IAsyncEnumerable<NetCord.Rest.RestMessage> messages = context.Channel.GetMessagesAsync();
        List<ulong> ids = new();

        await foreach (var message in messages)
        {
            ids.Add(message.Id);
        }
        Console.WriteLine($"Deleting {ids.Count} messages in channel {context.Channel.Id}");

        //await vega.Client.Rest.DeleteMessagesAsync(context.Channel.Id, ids.ToArray());

        await context.Interaction.SendResponseAsync(
            NetCord.Rest.InteractionCallback.Message(
                new NetCord.Rest.InteractionMessageProperties
                {
                    Content = "Cleared messages!"
                }
            )
        );
    }
}