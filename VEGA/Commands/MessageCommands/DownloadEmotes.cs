using System.Text.RegularExpressions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Models;
using System.IO.Compression;
using NetCord;
using NetCord.Services;

namespace MessageCommands;

public class DownloadEmotes : ApplicationCommandModule<ApplicationCommandContext>
{
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.AttachFiles)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.AttachFiles)]
    [MessageCommand("DownloadEmotes")]
    public async Task Execute(RestMessage message) {

        var msgRef = message.MessageSnapshots.FirstOrDefault() ?? null;
        string input = msgRef?.Message.Content ?? message.Content;
        
        List<CustomEmote> emotes = ParseEmotes(input);

        // Business validations
        if (emotes.Count == 0)
            throw new BusinessException("No emote found in message.");
        if (emotes.Count > 20)
            throw new BusinessException("Too many emotes found in message (max 20).");

        using HttpClient client = new HttpClient();
        // Download all PNGs concurrently
        var downloadTasks = new List<Task<byte[]>>();
        foreach (var url in emotes.Select(e => e.Url))
        {
            downloadTasks.Add(client.GetByteArrayAsync(url));
        }
        byte[][] pngBytesArray = await Task.WhenAll(downloadTasks);

        // Create Zip in memory
        using var memoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            for (int i = 0; i < pngBytesArray.Length; i++)
            {
                var zipEntry = zipArchive.CreateEntry($"emote{i + 1}.png");
                using var entryStream = zipEntry.Open();
                await entryStream.WriteAsync(pngBytesArray[i]);
            }
        }
        memoryStream.Seek(0, SeekOrigin.Begin);

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Content = string.Format("Here you are, {0} emote{1} !", emotes.Count, emotes.Count > 1 ? "s" : ""),
                    Attachments = new[]
                    {
                        new AttachmentProperties("emotes.zip", memoryStream)
                    }
                }
            )
        );
    }

    private static List<CustomEmote> ParseEmotes(string msgContent) {
        Regex findEmotesRegex = new Regex("<(a)?:(.*?):(.*?)>", RegexOptions.ECMAScript);
        MatchCollection matches = findEmotesRegex.Matches(msgContent);

        List<CustomEmote> emoteList = new();

        foreach (Match match in matches)
        {
            var emote = match.Value;
            var cleanupRegex = new Regex("[<,>]", RegexOptions.ECMAScript);
            emote = cleanupRegex.Replace(emote, "");

            var parts = emote.Split(':').Where(x => x != "").ToList();

            //<:huh:1293177600025301062>

            bool isAnimated = parts.Count > 2;
            string name = parts[0];
            string id = parts[1];
            string filename = string.Format("{0}.{1}", name, isAnimated ? "gif" : "png");
            string url = string.Format("https://cdn.discordapp.com/emojis/{0}.{1}?size=512&quality=lossless", id, isAnimated ? "gif" : "png");

            // If emote already in list, don't add it to list
            if (!emoteList.Exists(y => y.Id == id))
            {
                emoteList.Add(new CustomEmote(isAnimated, id, name, filename, url));
            }
        }
        return emoteList;
        
    }
}