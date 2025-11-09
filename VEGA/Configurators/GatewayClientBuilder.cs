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
    public class GatewayClientBuilder
    {
        private readonly GatewayClient _client;

        private GatewayClientBuilder(string token)
        {
            _client = new GatewayClient(new BotToken(token), new GatewayClientConfiguration
            {
                Intents = GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent,
                Logger = new ConsoleLogger(),
            });
        }

        public static GatewayClientBuilder Create(string token)
        {
            return new GatewayClientBuilder(token);    
        } 

        /// <summary>
        /// Builds the configured client and registers application commands (async because registration calls the API).
        /// Returns the created GatewayClient and the configured ApplicationCommandService.
        /// </summary>
        public GatewayClient Build()
        {
            return _client;
        }
    }
}
