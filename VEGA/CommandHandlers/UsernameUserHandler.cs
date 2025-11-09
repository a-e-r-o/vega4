using NetCord;
using NetCord.Gateway;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class UsernameUserHandler : IUserCommandHandler
{
    public string Name => "username";
    public string Description => "Returns the username of the message author.";

    public Task<string> CommandDelegate(User user, Vega vega)
    {
        return Task.FromResult(user.Username);
    }
}