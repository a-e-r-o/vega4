using System.Threading.Tasks;
using NetCord.Rest;
using NetCord.Gateway;

namespace Handlers;

public static class MiscHandlers
{
    public static async Task Connected(GatewayClient client)
    {
        Console.WriteLine("Connected");
        Console.WriteLine("Guilds :");
        
        var asyncGuildList = client.Rest.GetCurrentUserGuildsAsync();
        await foreach (var guild in asyncGuildList)
        {
            Console.WriteLine($"{guild.Id} - {guild.Name}");
        }
    }

    public static void Connecting(GatewayClient client)
    {
        Console.WriteLine("Connecting...");
    }
}