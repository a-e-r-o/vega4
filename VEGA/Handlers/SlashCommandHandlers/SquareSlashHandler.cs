using Handlers;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class SquareHandler : SlashCommandBase
{
    public override string Name => "square";
    public override string Description => "Squares a number. Because you can't do it yourself on the Windows calculator, it seems.";
    public override Delegate CommandDelegate => Execute;
    public SquareHandler(Vega vegaInstance) : base(vegaInstance){}

    public async Task Execute(ApplicationCommandContext context, int value)
    {
        var content = $"{value}² = {value * value}";
        await context.Interaction.SendResponseAsync(
            InteractionCallback.Message(content)
        );
    }
}