using System.ComponentModel.DataAnnotations;

namespace Models.Entities;


public class GuildSettings
{
    [Key]
    public ulong GuildId { get; set; }  // PK, ulong mappe à bigint en PG

    public List<Trigger> Triggers { get; set; } = new List<Trigger>();  // Navigation one-to-many
}
