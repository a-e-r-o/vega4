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

        string resMsg = string.Empty;

        foreach(var trigger in settings.Triggers)
        {
            resMsg += string.Format("{0}, {1}, {2}, {3}", trigger.Pattern, trigger.Response, trigger.RegexOptions, trigger.PingOnReply);
        }

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(resMsg)
        );
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
        var guildId = Context.Interaction.GuildId ?? throw new BusinessException("Unable to retrieve guild");

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
        [SlashCommandParameter(Name = "id", Description = "ID of the trigger to delete")] int triggerId
    )
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message($"removed trigger {triggerId}")
        );
    }

}