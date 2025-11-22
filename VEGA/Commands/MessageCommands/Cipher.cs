using System.Text.RegularExpressions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Models;

namespace MessageCommands;

public class CipherMsg : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Cipher")]
    public async Task Cipher(RestMessage message) {
        await Context.Client.Rest.DeleteMessageAsync(message.ChannelId, message.Id);
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message("WIP")
        );
    }

    [MessageCommand("Uncipher")]
    public async Task Uncipher(RestMessage message) {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message("WIP")
        );
    }

    private static string Cipher(string msgContent) {
        // Implement your cipher logic here
        return msgContent; // Placeholder return
    }
}