namespace Core.Models;

public class Configuration
{
    public Configuration(string botToken)
    {
        BotToken = botToken;
    }
    
    public string BotToken { get; set; }
}