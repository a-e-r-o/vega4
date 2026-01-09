using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Core;
using Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models.Entities;

namespace Services;

public class FeedService(AppDbContext dbContext, IMemoryCache cache)
{
    private readonly AppDbContext _dbContext = dbContext;
    private readonly IMemoryCache _cache = cache;

    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _timersCancellationTokens = new();
    private readonly IList<FeedProperties> FeedPropertiesList = new List<FeedProperties>();

    public void AddNewFeed(FeedProperties feedProperties)
    {
        // TODO : save FeedProperties to BDD

        CancellationTokenSource ccts = new();
        
        _timersCancellationTokens.AddOrUpdate
        (
            feedProperties.Id,
            ccts,
            (key, existingValue) => ccts
        );

        _ = InitiateTimer(feedProperties, ccts);
    }

    public async Task InitiateTimer(FeedProperties feedProperties, CancellationTokenSource ccts)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        try
        {
            while (await timer.WaitForNextTickAsync(ccts.Token))
            {
                Console.WriteLine("Hello World after 10 seconds");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Timer cancelled");
        }
    }
}