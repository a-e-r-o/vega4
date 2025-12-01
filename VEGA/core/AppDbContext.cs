using Core.Models;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Core;

public class AppDbContext : DbContext
{
    public DbSet<GuildSettings> GuildSettings { get; set; }
    public DbSet<Trigger> Triggers { get; set; }

    private Configuration _config { get; }


    public AppDbContext(Configuration config)
    {
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_config.DbConnexionString);
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table names are derived from Entity name, using Snake Case convention
        modelBuilder.Entity<GuildSettings>()
                    .HasKey(g => g.GuildId);    // Primary Key

        modelBuilder.Entity<Trigger>()
                    .HasKey(t => t.TriggerId);  // Primary Key

        modelBuilder.Entity<Trigger>()
                    .Property(t => t.TriggerId)
                        .HasDefaultValueSql("gen_random_uuid()");  // Default value : new Guid 

        modelBuilder.Entity<GuildSettings>()
                    .HasMany(g => g.Triggers)           // Trigger list
                    .WithOne()                          // Navigation property not needed
                    .HasForeignKey(t => t.GuildId)      // Foreign Key
                    .OnDelete(DeleteBehavior.Cascade);  // On delete cascade
    }
}