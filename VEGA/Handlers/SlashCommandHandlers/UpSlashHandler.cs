using NetCord.Services.ApplicationCommands;
using NetCord.Rest;
using VEGA.Core;

namespace Handlers;

public class UpSlashHandler : ISlashCommandHandler
{
    public string Name => "up";
    public string Description => "Indicates uptime and other infos about the bot.";
    public async Task CommandDelegate(ApplicationCommandContext context, Vega vega)
    {
        var embed = new NetCord.Rest.EmbedProperties
        {
            Title = "V E G A",
            Fields = new[]
            {
                new NetCord.Rest.EmbedFieldProperties
                {
                    Name = "Uptime",
                    Value = (DateTime.UtcNow - vega.StartTime).ToString(@"dd\.hh\:mm\:ss"),
                    Inline = false,
                },
                new NetCord.Rest.EmbedFieldProperties
                {
                    Name = "Created at",
                    Value = string.Format
                            (
                                "{0} UTC",
                                vega.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                vega.StartTime
                            ),
                    Inline = false
                },
            },
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