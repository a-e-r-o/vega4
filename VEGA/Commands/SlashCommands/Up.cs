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
        var vegaInstance = Core.ServiceRegistry.ServiceProvider!.GetService<Vega>()!;

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
                    Value = (DateTime.UtcNow - vegaInstance.StartTime).ToString(@"dd\.hh\:mm\:ss")
                },
                new EmbedFieldProperties
                {
                    Name = "Started at",
                    Value = string.Format
                            (
                                "{0} UTC",
                                vegaInstance.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
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