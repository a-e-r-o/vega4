using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Commands;

public class Up : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("up", "Indicates uptime and other infos about the bot")]
    public async Task Execute()
    {
        var self = await Context.Client.Rest.GetUserAsync(Context.Client.Id);

        var embed = new EmbedProperties
        {
            Title = "ᴠ.ᴇ.ɢ.ᴀ.",
            Url = "https://github.com/a-e-r-o/vega4",
            Color = new Color(96, 240, 213),
            Thumbnail = new EmbedThumbnailProperties(self.GetAvatarUrl()?.ToString()),
            Fields = new[]
            {
                new EmbedFieldProperties
                {
                    Name = "Uptime",
                    //Value = (DateTime.UtcNow - vega.StartTime).ToString(@"dd\.hh\:mm\:ss")
                    Value = "WIP"
                },
                new EmbedFieldProperties
                {
                    Name = "Started at",
                    Value = string.Format
                            (
                                "{0} UTC",
                                "WIP"
                                //vega.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                //vega.StartTime
                            )
                }
            },
            Footer = new EmbedFooterProperties
            {
                Text = "UwU"
            }
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