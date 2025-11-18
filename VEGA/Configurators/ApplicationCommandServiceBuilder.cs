using System.Reflection;
using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;
using VEGA.Core;

namespace Configurators
{
    /// <summary>
    /// Small fluent builder to create and configure a GatewayClient together with an ApplicationCommandService.
    /// </summary>
    public class ApplicationCommandServiceBuilder
    {
        private ApplicationCommandService<ApplicationCommandContext> _appCommandService;

        private ApplicationCommandServiceBuilder()
        {
            _appCommandService = new ApplicationCommandService<ApplicationCommandContext>();
        }

        public static ApplicationCommandServiceBuilder Create()
        {
            return new ApplicationCommandServiceBuilder();
        }

        public ApplicationCommandServiceBuilder AddCommandHandlers(Vega VegaInstance)
        {
            var commandModules = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => (t.Namespace == "SlashCommands" || t.Namespace == "UserCommands" || t.Namespace == "MessageCommands")
                        && t.IsClass
                        && !t.IsAbstract
                        && typeof(ApplicationCommandModule<ApplicationCommandContext>).IsAssignableFrom(t));

            foreach (var module in commandModules)
            {
                _appCommandService.AddModule(module);            
            }

            // Add any default configuration here if needed
            return this;
        }

        public async Task<ApplicationCommandService<ApplicationCommandContext>> BuildAsync(ShardedGatewayClient client)
        {
            // Register all commands to Discord
            await _appCommandService.RegisterCommandsAsync(client.Rest, client.Id);
            
            return _appCommandService;
        }
    }
}
