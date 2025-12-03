using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


public class Trigger
{
    [Key]   // PK
    public Guid TriggerId { get; set; } = Guid.Empty;

    [Required]
    public ulong GuildId { get; set; } = 0; // FK -> GuildSettings

    [Required, MaxLength(255)]
    public string Pattern { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Response { get; set; } = string.Empty;

    public int RegexOptions { get; set; } = 0;

    public bool PingOnReply { get; set; } = false;

    private Trigger(){}

    public Trigger(ulong guildId, string pattern, string response, int regexOptions, bool pingOnReply = false, Guid? triggerId = null)
    {
        GuildId = guildId;
        Pattern = pattern;
        Response = response;
        RegexOptions = regexOptions;
        PingOnReply = pingOnReply;
        TriggerId = triggerId ?? Guid.NewGuid();
    }

}