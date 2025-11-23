using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

public class DiceRoll : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("diceroll", "Roll a dice")]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "faces", Description = "Number of sides in the dice", MinValue = 2, MaxValue = 100
        )] int diceFaces = 6,
        [SlashCommandParameter(
            Name = "rolls", Description = "Number dices to roll", MinValue = 1, MaxValue = 100
        )] int rollCount = 1
    )
    {
        List<string> results = new();

        for (int i = 0; i < rollCount; i++)
        {
            int randInt = Random.Shared.Next(1, diceFaces + 1);
            results.Add($"[{randInt}]");
        }

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                string.Join("  ", results)
            )
        );
    }
}