using System.Reflection;
using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using NetCord.Services.ApplicationCommands;
using NetCord.Services;
using NetCord.Rest;
using System.Threading.Tasks;
using NetCord.Services.Commands;

namespace Configurators
{
    /// <summary>
    /// Small fluent builder to create and configure a GatewayClient together with an ApplicationCommandService.
    /// </summary>
    public class ShardedGatewayClientBuilder
    {
        private readonly ShardedGatewayClient _client;

        private ShardedGatewayClientBuilder(string token)
        {
            _client = new ShardedGatewayClient
            (
                new BotToken(token), new ShardedGatewayClientConfiguration
                {
                    IntentsFactory = (shard) => GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildUsers,
                    LoggerFactory = ShardedConsoleLogger.GetFactory()
                }
            );
        }

        public static ShardedGatewayClientBuilder Create(string token)
        {
            return new ShardedGatewayClientBuilder(token);    
        } 

        /// <summary>
        /// Builds the configured client and registers application commands (async because registration calls the API).
        /// Returns the created GatewayClient and the configured ApplicationCommandService.
        /// </summary>
        public ShardedGatewayClient Build()
        {
            return _client;
        }
    }
}
