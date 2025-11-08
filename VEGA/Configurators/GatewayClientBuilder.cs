using System.Reflection;
using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using NetCord.Services.ApplicationCommands;
using NetCord.Services;
using NetCord.Rest;
using System.Threading.Tasks;

namespace Configurators
{
    /// <summary>
    /// Small fluent builder to create and configure a GatewayClient together with an ApplicationCommandService.
    /// </summary>
    public class GatewayClientBuilder
    {
        private readonly GatewayClient _client;
        private ApplicationCommandService<ApplicationCommandContext>? _commandService;
        private Assembly? _modulesAssembly;

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

        public GatewayClientBuilder ConfigureCommands(Assembly modulesAssembly)
        {
            _modulesAssembly = modulesAssembly;
            _commandService = new ApplicationCommandService<ApplicationCommandContext>();

            // minimal example commands
            _commandService.AddSlashCommand(new SlashCommandBuilder("ping", "Ping!", () => "Pong!"));
            _commandService.AddUserCommand(new UserCommandBuilder("Username", (User user) => user.Username));
            _commandService.AddMessageCommand(new MessageCommandBuilder("Length", (RestMessage message) => message.Content.Length.ToString()));

            return this;
        }

        public GatewayClientBuilder RegisterHandlers()
        {
            if (_commandService == null)
                _commandService = new ApplicationCommandService<ApplicationCommandContext>();

            _client.Connect += async () => Console.WriteLine("connected");
            _client.Connecting += async () => Console.WriteLine("connecting");

            _client.InteractionCreate += async interaction =>
            {
                if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
                    return;

                var result = await _commandService!.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, _client));

                if (result is not IFailResult failResult)
                    return;

                try
                {
                    await interaction.SendResponseAsync(InteractionCallback.Message(failResult.Message));
                }
                catch
                {
                }
            };

            return this;
        }

        /// <summary>
        /// Builds the configured client and registers application commands (async because registration calls the API).
        /// Returns the created GatewayClient and the configured ApplicationCommandService.
        /// </summary>
        public async Task<(GatewayClient client, ApplicationCommandService<ApplicationCommandContext> service)> BuildAsync()
        {
            if (_modulesAssembly != null && _commandService != null)
            {
                _commandService.AddModules(_modulesAssembly);
            }

            // Register commands with REST; this requires the client to exist and its Id to be available
            if (_commandService != null)
            {
                var list = await _commandService.RegisterCommandsAsync(_client.Rest, _client.Id);
                Console.WriteLine($"Registered {list.Count} commands.");
                Console.WriteLine(string.Format("Registered commands {0}", string.Join(',', list)));
            }

            return (_client, _commandService ?? new ApplicationCommandService<ApplicationCommandContext>());
        }
    }
}
