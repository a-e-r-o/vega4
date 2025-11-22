using Core.Models;
using Microsoft.EntityFrameworkCore;
using Models.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Core;

public class AppDbContext : DbContext
{
    public DbSet<GuildSettings> GuildSettings { get; set; }
    public DbSet<Trigger> Triggers { get; set; }

    public AppDbContext(VegaConfiguration config)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Database=ma_db;Username=mon_user;Password=mon_mdp")
                        .UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildSettings>()
                    .ToTable("guild_settings")  // Nom de table exact
                    .HasKey(g => g.GuildId);  // PK explicite

        modelBuilder.Entity<Trigger>()
                    .ToTable("triggers")  // Nom de table exact
                    .HasKey(t => t.TriggerId);  // PK

        modelBuilder.Entity<Trigger>()
                    .Property(t => t.TriggerId)
                        .HasDefaultValueSql("gen_random_uuid()");  // Auto-génère UUID en PG si pas fourni

        modelBuilder.Entity<GuildSettings>()
                    .HasMany(g => g.Triggers)  // Collection
                    .WithOne()                 // Pas de navigation inverse
                    .HasForeignKey(t => t.GuildId)  // FK
                    .OnDelete(DeleteBehavior.Cascade);  // Cascade si voulu
    }
}