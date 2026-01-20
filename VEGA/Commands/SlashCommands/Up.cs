using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Core;
using Microsoft.Extensions.DependencyInjection;

namespace SlashCommands;

public class Up : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("up", "Indicates uptime and other infos about the bot")]
    public async Task Execute()
    {
        var self = await Context.Client.Rest.GetUserAsync(Context.Client.Id);

        var uptime = DateTime.UtcNow - GlobalRegistry.StartTime;
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
                    Value = string.Format("{0} days, {1}h, {2}m, {3}s", uptime.Days, uptime.Hours, uptime.Minutes, uptime.Seconds)
                },
                new EmbedFieldProperties
                {
                    Name = "Started at",
                    Value = string.Format
                            (
                                "{0} UTC",
                                GlobalRegistry.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
                            )
                }
            },
            Footer = new EmbedFooterProperties
            {
                Text = "Be like the penguin. March to the mountains"
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