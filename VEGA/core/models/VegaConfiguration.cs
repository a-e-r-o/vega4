namespace Core.Models;

public class VegaConfiguration
{
    public VegaConfiguration(string botToken)
    {
        BotToken = botToken;
    }
    
    public string BotToken { get; set; }
}