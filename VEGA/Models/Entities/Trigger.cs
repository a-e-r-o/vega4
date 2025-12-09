using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.Entities;

public class Trigger
{
    [Key]   // PK
    public Guid TriggerId { get; set; }

    [Required]
    public ulong GuildId { get; set; } = 0; // FK -> GuildSettings

    /// <summary>
    /// Regex
    /// </summary>
    [Required, MaxLength(255)]
    public string Pattern { get; set; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Response { get; set; } = string.Empty;

    /// <summary>
    /// Regex options as sum of the regex options flags in dotnet 
    /// </summary>
    public int RegexOptions { get; set; } = 0;

    /// <summary>
    /// Reply with ping to message that triggered the response
    /// </summary>
    public bool PingOnReply { get; set; } = false;

    /// <summary>
    /// UTC
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    private Trigger(){}

    public Trigger(
        ulong guildId, string pattern,
        string response,
        int regexOptions,
        bool pingOnReply = false,
        DateTime? createdAt = null
    )
    {
        GuildId = guildId;
        Pattern = pattern;
        Response = response;
        RegexOptions = regexOptions;
        PingOnReply = pingOnReply;
        CreatedAt = createdAt ?? DateTime.UtcNow;
    }

}