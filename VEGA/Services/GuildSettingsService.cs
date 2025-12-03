using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Entities;

public class GuildSettingsService
{
    private readonly AppDbContext _dbContext;
    private readonly IMemoryCache _cache;

    // Consts related to cache
    private const int CACHE_LIFETIME_IN_MINUTES = 10;
    private const string CACHE_PREFIX = "guildSettings_";

    // Getter to normalize cache key structure
    private string GetCacheKey(ulong guildId) => CACHE_PREFIX + guildId; 

    public GuildSettingsService(AppDbContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }


    /// <summary>
    /// Get a GuildSettings object. Attempts to retrieve it from cache, if not found in cache,
    /// fetch it in DB. If not in DB either, return a new GuildSettings object with default values.
    /// </summary>
    public async Task<GuildSettings> GetByIdAsync(ulong guildId)
    {
        var cacheKey = GetCacheKey(guildId);

        // Found in cache
        if (_cache.TryGetValue(cacheKey, out GuildSettings? cachedSettings))
            return cachedSettings!;

        var dbSettings = await _dbContext.GuildSettings
                                    .Include(g => g.Triggers) // Eagerly load Triggers
                                    .FirstOrDefaultAsync(g => g.GuildId == guildId);
        // Found in BDD
        if (dbSettings != null)
        {
            _cache.Set(cacheKey, dbSettings, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_LIFETIME_IN_MINUTES)));

            return dbSettings;
        }
        // Found in neither BDD nor cache
        else
        {
            // New object with default values
            return new GuildSettings(guildId);
        }
    }


    /// <summary>
    /// SaveOrUpdate a GuildSettings object in cache and DB
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="newSettings"></param>
    /// <returns></returns>
    /// <exception cref="BusinessException"></exception>
    public async Task<GuildSettings> SaveOrUpdateAsync(ulong guildId, GuildSettings newSettings)
    {
        // Second level validation -> should have been checked a first time in business code
        if (guildId != newSettings.GuildId)
            throw new BusinessException("GuildId and GuildSettings ID mismatch");

        GuildSettings? existingSettings = _dbContext.GuildSettings.Find(guildId);
        if (existingSettings is null)
        {
            // Doesn't exist, so add
            _dbContext.GuildSettings.Add(newSettings);
        }
        else
        {
            // Exists, so update
            _dbContext.Entry(existingSettings).CurrentValues.SetValues(newSettings);
        }

        await _dbContext.SaveChangesAsync();

        // Update cache
        var key = GetCacheKey(guildId);
        _cache.Set(key, newSettings);

        return newSettings;
    }


    /// <summary>
    /// Add a trigger on the targeted GuildSettings
    /// </summary>
    /// <param name="guildId"></param>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public async Task<GuildSettings> AddTrigger(ulong guildId, Trigger trigger)
    {
        GuildSettings settings = await GetByIdAsync(guildId);
        settings.Triggers.Add(trigger);
        GuildSettings updatedSettings = await SaveOrUpdateAsync(guildId, settings);

        return settings;
    }
}