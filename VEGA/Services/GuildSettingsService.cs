using Core;
using Microsoft.Extensions.Caching.Memory;
using Models.Entities;

public class GuildSettingsService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;

    private const int CACHE_LIFETIME_IN_MINUTES = 10;
    private const string CACHE_PREFIX = "guildSettings_";

    public GuildSettingsService(AppDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<GuildSettings?> GetByIdAsync(ulong guildId)
    {
        var key = $"{CACHE_PREFIX}{guildId}";

        if (_cache.TryGetValue(key, out GuildSettings? cached))
            return cached;

        var settings = await _dbContext.GuildSettings.FindAsync(guildId);
        if (settings != null)
        {
            _cache.Set(key, settings, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_LIFETIME_IN_MINUTES)));
        }
        return settings;
    }

    public async Task UpdateAsync(ulong guildId, GuildSettings newSettings)
    {
        _dbContext.GuildSettings.Update(newSettings);
        await _dbContext.SaveChangesAsync();

        var key = $"{CACHE_PREFIX}{guildId}";
        _cache.Set(key, newSettings);
    }
}