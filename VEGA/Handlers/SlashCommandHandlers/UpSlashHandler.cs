using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class UpSlashHandler : ISlashCommandHandler
{
    public string Name => "up";
    public string Description => "Indicates uptime and other infos about the bot.";
    public async Task Execute(ApplicationCommandContext context, Vega vega)
    {
        var self = await context.Client.Rest.GetUserAsync(context.Client.Id);

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
                    Value = (DateTime.UtcNow - vega.StartTime).ToString(@"dd\.hh\:mm\:ss")
                },
                new EmbedFieldProperties
                {
                    Name = "Started at",
                    Value = string.Format
                            (
                                "{0} UTC",
                                vega.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                vega.StartTime
                            )
                }
            },
            Footer = new EmbedFooterProperties
            {
                Text = "UwU"
            }
        };

        await context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                new InteractionMessageProperties
                {
                    Embeds = new[] { embed }
                }
            )
        );
    }
}