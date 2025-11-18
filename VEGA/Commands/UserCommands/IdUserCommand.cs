using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace UserCommands;

public class IdModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [UserCommand("ID")]
    public static string Id(User user) => user.Id.ToString();
}