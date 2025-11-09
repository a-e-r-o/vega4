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
    Task<string> CommandDelegate(Vega vega);
}

public interface IMessageCommandHandler : IHandlerBase
{
    Task<string> CommandDelegate(RestMessage message, Vega vega);
}

public interface IUserCommandHandler : IHandlerBase
{
    Task<string> CommandDelegate(User user, Vega vega);
}