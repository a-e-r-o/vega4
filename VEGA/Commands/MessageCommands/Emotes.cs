using System.Text.RegularExpressions;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Models;

namespace MessageCommands;

public class Emotes : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("DownloadEmotes")]
    public async Task Timestamp(RestMessage message) {

        //"<:huh:1293177600025301062> <:77487merushyblush:1384904793872531506>"
        var msgRef = message.MessageSnapshots?[0];
        List<CustomEmote> emotes = new();
        string response;
        
        if (msgRef is not null)
        {
            emotes = ParseEmotes(msgRef.Message.Content);
        }
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


/*
matches?.forEach(x => {
    x = x.replace(/[<,>]/g, '')
    const parts = x.split(':').filter(x => x != '')

    const anim = parts.length > 2
    const id = parts.pop()
    const name = parts.pop()
    const filename = `${name}.${anim?'gif':'png'}`
    const url = `https://cdn.discordapp.com/emojis/${id}.${anim?'gif':'png'}?size=512&quality=lossless`

    //https://cdn.discordapp.com/emojis/1384904793872531506.png?size=512&quality=lossless

    // If parsed data is malfrmed, abort
    if (!id || !name || !url)
        return
    // If emote is a duplicate, abort
    if (emotes.find(y => y.id == id))
        return

    emotes.push({
        animated: anim,
        id: id,
        name: name,
        url: url,
        filename: filename
    })
*/