namespace Core.Models;

public class Configuration
{
    public Configuration(string botToken, string dbConnexionString)
    {
        BotToken = botToken;
        DbConnexionString = dbConnexionString;
    }
    
    public string BotToken { get; set; }
    public string DbConnexionString { get; set; }
}