using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using NetCord.Gateway;
using NetCord.Rest;

namespace Handlers;

public class MessageCreateHandler
{
    //private readonly ICacheService _cache; // ton cache singleton
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageCreateHandler> _logger;

    public MessageCreateHandler(/*ICacheService cache,*/ ILogger<MessageCreateHandler> logger)
    {
        //_cache = cache;
        _logger = logger;
    }

    public async Task MessageCreate(GatewayClient client, Message message)
    {
        Console.WriteLine("Message received in {0} from {1}", message.ChannelId, message.Author.Username);

        // interaction rapide avec le cache singleton
        //var cached = _cache.GetSomething(message.Author.Id);

        // pour la BDD (scoped) : créer un scope par invocation
        //using var scope = _scopeFactory.CreateScope();
        //var dbService = _serviceProvider.GetRequiredService<IMyScopedDbService>();

        // utiliser dbService de façon asynchrone
        //await dbService.HandleMessageAsync(message, cached);
    }
}