using Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Models.Entities;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using static Core.GlobalRegistry;

namespace SlashCommands;

[SlashCommand("trigger", "Manage triggers patterns for this server")]
public class Triggers : ApplicationCommandModule<ApplicationCommandContext>
{

    [SubSlashCommand("list", "List triggers on this server")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    public async Task List()
    {
        GuildSettingsService service = MainServiceProvider.GetRequiredService<GuildSettingsService>();
        GuildSettings settings = await service.GetByIdAsync(Context.Interaction.Guild!.Id);

        var triggers = settings.Triggers.OrderByDescending(x => x.CreatedAt)
                                        .ToList();

        List<string> resMessages = [];

        if (triggers.Count == 0)
        {
            resMessages.Add("There are no triggers on this server");
        }
        else
        {
            resMessages.Add("## Current triggers on this server");

            for (int i = 0; i < triggers.Count; i++)
            {
                string currentTriggerInfo = "";
                
                Trigger iTgr = triggers[i];
                currentTriggerInfo += $"\n{i}.";
                currentTriggerInfo += $"\n> **Pattern** : `{iTgr.Pattern}`";
                currentTriggerInfo += $"\n> **Response** : `{iTgr.Response}`";
                currentTriggerInfo += $"\n> **Regex options** : `{iTgr.RegexOptions}`";
                currentTriggerInfo += $"\n> **Ping on reply** : {iTgr.PingOnReply}";

                // Discord message char limit is 2000
                if (resMessages.Last().Length + currentTriggerInfo.Length > 2000)
                {
                    // If current message would exeed limit with this trigger, add it to the next message instead
                    resMessages.Add(string.Empty);
                } 
                resMessages[^1] += currentTriggerInfo;
            }
        }

        // Send initial message response
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(resMessages[0])
        );

        // Send eventual additionnal messages
        if (resMessages.Count > 1)
        {
            for (int i = 1; i < resMessages.Count; i++)
            {
                await Context.Channel.SendMessageAsync(resMessages[i]);
            }
        }
    }

    [SubSlashCommand("add", "Add a new trigger")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    public async Task Add(
        [SlashCommandParameter(
            Name = "regex",
            Description = "Pattern to match using regex notation",
            MinLength = 3, MaxLength = 50
        )] string regex,
        [SlashCommandParameter(
            Name = "response",
            Description = "Response message to send when pattern is detected",
            MinLength = 1, MaxLength = 2000
        )] string response,
        [SlashCommandParameter(
            Name = "regexoptions",
            Description = "Regex matching options flag : see .NET Regular expression options",
            MaxLength = 10
        )] int regexOptions = 0
    )
    {
        GuildSettingsService service = MainServiceProvider.GetRequiredService<GuildSettingsService>();
        var guildId = Context.Interaction.GuildId ?? throw new SlashCommandBusinessException("Unable to retrieve guild");

        Trigger newTrigger = new Trigger(guildId, regex, response, regexOptions);
        
        var newSettings = await service.AddTrigger(guildId, newTrigger);

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message($"Added trigger with pattern `/{regex}/{regexOptions}`")
        );
    }

    [SubSlashCommand("delete", "Delete a trigger by ID (see ID in trigger list)")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    [RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageMessages)]
    public async Task Delete(
        [SlashCommandParameter(
            Name = "id",
            Description = "ID of the trigger to delete"
        )] int triggerIndex
    )
    {
        GuildSettingsService service = MainServiceProvider.GetRequiredService<GuildSettingsService>();
        ulong guildId = Context.Interaction.GuildId ?? throw new SlashCommandBusinessException("Unable to retrieve guild");
        bool deleted = await service.DeleteTrigger(guildId, triggerIndex);

        if (!deleted)
            throw new SlashCommandBusinessException("Trigger deletion failed");

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message($"Removed trigger successfuly")
        );
    }

}