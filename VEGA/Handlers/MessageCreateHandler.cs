using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Gateway;
using NetCord.Rest;
using static Core.GlobalRegistry;
using Models.Entities;
using System.Text.RegularExpressions;

namespace Handlers;

public class MessageCreateHandler
{
    //private readonly ICacheService _cache; // ton cache singleton
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageCreateHandler> _logger;

    public MessageCreateHandler(/*ICacheService cache,*/ ILogger<MessageCreateHandler> logger)
    {
        //_cache = cache;
        _logger = logger;
    }

    public async Task MessageCreate(GatewayClient client, Message message)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Check if message channel exists and is in guild ans is not from a bot
        if (message.GuildId.HasValue && message.Channel is not null && !message.Author.IsBot)
        {
            GuildSettingsService service = MainServiceProvider.GetRequiredService<GuildSettingsService>();
            GuildSettings settings = await service.GetByIdAsync(message.GuildId.Value);

            // No triggers set for this guild
            if (settings.Triggers.Count != 0)
            {
                string res = checkTriggers(message, settings);

                if (res != string.Empty)
                    await message.Channel.SendMessageAsync(res);
            }
        }

        stopwatch.Stop();
        Console.WriteLine("MessageCreate handled in {0} ms", stopwatch.ElapsedMilliseconds);
    }

    private string checkTriggers(Message msg, GuildSettings settings){
        Trigger foundPattern;

        // Find if the message matches any trigger pattern
        for (var i = 0; i < settings.Triggers.Count; i++) {
            var pattern = settings.Triggers[i];
            try {
                if (Regex.IsMatch(msg.Content, pattern.Pattern, (RegexOptions)pattern.RegexOptions))
                {
                    foundPattern = pattern;
                    return foundPattern.Response;
                }
            } 
            catch (Exception ex)
            {
                
            }
        }

        return string.Empty;
    }
}