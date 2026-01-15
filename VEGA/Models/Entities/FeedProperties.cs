using System.ComponentModel.DataAnnotations;

namespace Models.Entities;

public class FeedProperties
{
    [Key]   // PK
    public Guid FeedId { get; set; }
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }

    /// <summary>
    /// Flags for future use
    /// </summary>
    public int Params { get; set; }
    public string Topic { get; set; } = "";
    public int IntervalInMinutes { get; set;}

    /// <summary>
    /// Minute of the hour to start the interval at
    /// </summary>
    public int StartAtMinute { get; set;}

    public DateTime CreatedAt { get; set; }

    private FeedProperties(){}
    
    public FeedProperties(
        ulong guildId,
        ulong channelId,
        string topic,
        int intervalInMinutes,
        int startAtMinute = -1,
        int parameters = 0,
        DateTime? createdAt = null
    )
    {
        GuildId = guildId;
        ChannelId = channelId;
        Topic = topic;
        IntervalInMinutes = intervalInMinutes;
        StartAtMinute = startAtMinute;
        Params = parameters;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }
}

public class FeedPostReceit
{
    public Guid FeedId { get; set; }
    public string PostId { get; set; } = "";
    public DateTime PostedAt { get; set; }
}