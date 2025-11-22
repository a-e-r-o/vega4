using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


public class Trigger
{
    [Key]
    public Guid TriggerId { get; set; }  // PK

    [Required]
    public ulong GuildId { get; set; }  // FK -> GuildSettings

    [Required, MaxLength(255)]
    public string Pattern { get; set; }

    [Required, MaxLength(5000)]
    public string Response { get; set; }

    public string? RegexOptions { get; set; }

    public bool PingOnReply { get; set; }

}