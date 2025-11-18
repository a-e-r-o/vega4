using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Commands;

[SlashCommand("trigger", "Manage triggers patterns for this server")]
public class Triggers : ApplicationCommandModule<ApplicationCommandContext>
{

    [SubSlashCommand("list", "List triggers on this server")]
    public async Task List()
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message("The list of triggers")
        );
    }

    [SubSlashCommand("add", "Add a new trigger")]
    public async Task Add(
        [SlashCommandParameter(Name = "regex", Description = "Pattern to match using regex notation")] string regex,
        [SlashCommandParameter(Name = "response", Description = "Response message to send when pattern is detected")] string response,
        [SlashCommandParameter(Name = "regexoptions", Description = "Regex matching options")] string regexOptions = "gmi"
    )
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message($"Added trigger [ID] with pattern `/{regex}/{regexOptions}`")
        );
    }

    [SubSlashCommand("delete", "Delete a trigger by ID (see ID in trigger list)")]
    public async Task Delete(
        [SlashCommandParameter(Name = "id", Description = "ID of the trigger to delete")] int triggerId
    )
    {
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message($"removed trigger {triggerId}")
        );
    }

}