using NetCord;
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
    Task CommandDelegate(ApplicationCommandContext context, Vega vega);
}

public interface IMessageCommandHandler : IHandlerBase
{
    Task CommandDelegate(ApplicationCommandContext context, RestMessage message, Vega vega);
}

public interface IUserCommandHandler : IHandlerBase
{
    Task CommandDelegate(ApplicationCommandContext contect, User user, Vega vega);
}