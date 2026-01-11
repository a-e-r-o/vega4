public class FeedProperties
{
    public Guid FeedId { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public string Params { get; set; } = "";
    public string Topic { get; set; } = "";
    public int IntervalInMinutes { get; set;}
    
    public FeedProperties()
    {
        
    }
}