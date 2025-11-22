using System.Text.RegularExpressions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Models;

namespace MessageCommands;

public class DownloadEmotes : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("DownloadEmotes")]
    public async Task Execute(RestMessage message) {

        var msgRef = message.MessageSnapshots.FirstOrDefault() ?? null;
        string response;
        string input = msgRef?.Message.Content ?? message.Content;
        
        List<CustomEmote> emotes = ParseEmotes(input);

        if (emotes.Count > 0)
        {
            response = "Found the following emotes in the message:\n";
            foreach (var emote in emotes)
            {
                response += string.Format("- Name: {0}, URL: {1}\n", emote.Name, emote.Url);
            }
        }
        else
        {
            response = "No emotes found in the message.";
        }

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(response)
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