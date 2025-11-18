using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Commands;

public class Cointoss : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("cointoss", "Toss a coin")]
    public async Task Execute()
    {
        Random rnd = new Random();
        bool randomBool = rnd.Next(2) == 0;

        string headUrl = "https://fr-academic.com/pictures/frwiki/49/1_euro_France.png";
        string tailsUrl = "https://upload.wikimedia.org/wikipedia/it/a/aa/1_%E2%82%AC_2006.png";

        var embed = new EmbedProperties
        {
            Thumbnail = new EmbedThumbnailProperties(randomBool ? headUrl : tailsUrl),
        };

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Embeds = new[] { embed }
                }
            )
        );
    }
}