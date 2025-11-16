using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public interface IHandlerBase
{
    string Name { get; }
    string Description { get; }
}

public interface ISlashCommandHandler : IHandlerBase
{
    //Func<ApplicationCommandContext, Vega, int, Task> CommandDelegate {get;}
    Task Execute(ApplicationCommandContext context, Vega vega);
}

public abstract class SlashCommandBase
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    protected Vega VegaInstance {get; private set;}

    public abstract Delegate CommandDelegate { get; }

    protected SlashCommandBase(Vega vegaInstance)
    {
        VegaInstance = vegaInstance;
    }
}

public interface IMessageCommandHandler : IHandlerBase
{
    Task Execute(ApplicationCommandContext context, RestMessage message, Vega vega);
}

public interface IUserCommandHandler : IHandlerBase
{
    Task Execute(ApplicationCommandContext context, User user, Vega vega);
}