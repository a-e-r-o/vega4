using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Handlers;

public class LengthMsgHandler : IMessageCommandHandler
{
    public string Name => "length";
    public string Description => "Counts the number of characters in a message";

    public Task<string> CommandDelegate(RestMessage message, Vega vega)
    {
       return Task.FromResult(message.Content.Length.ToString());
    }
}