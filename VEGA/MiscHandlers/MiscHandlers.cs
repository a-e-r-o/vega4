using NetCord.Gateway;

public static class MiscHandlers
{
    public static void Connected(GatewayClient client)
    {
        Console.WriteLine("Connected");
    }

    public static void Connecting(GatewayClient client)
    {
        Console.WriteLine("Connecting...");
    }
}