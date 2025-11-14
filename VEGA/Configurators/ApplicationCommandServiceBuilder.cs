using System.Reflection;
using NetCord;
using NetCord.Gateway;
using NetCord.Logging;
using NetCord.Services.ApplicationCommands;
using NetCord.Services;
using NetCord.Rest;
using System.Threading.Tasks;
using NetCord.Services.Commands;
using Handlers;
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
            var handlerTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "Handlers"
                        && t.IsClass
                        && !t.IsAbstract
                        && typeof(IHandlerBase).IsAssignableFrom(t));
            var handlers = new List<IHandlerBase>();
                        
            foreach (var type in handlerTypes)
            {
                try
                {
                    var handler = Activator.CreateInstance(type) as IHandlerBase;

                    if (handler != null)
                    {
                        Console.WriteLine($"Registering handler: {type.Name}");
                        handlers.Add(handler);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create instance of handler: {type.Name}");
                        throw new Exception("Null handler type");
                    }
                }
                catch
                {
                    Console.WriteLine($"Failed to create instance of handler: {type.Name}");
                    continue;
                }
            }

            foreach (var handler in handlers)
            {
                switch (handler)
                {
                    case ISlashCommandHandler slashCommandHandler:
                        _appCommandService.AddSlashCommand(
                            new SlashCommandBuilder(
                                slashCommandHandler.Name,
                                slashCommandHandler.Description,
                                (ApplicationCommandContext context) => slashCommandHandler.CommandDelegate(context, VegaInstance)
                            )
                        );
                        break;
                    case IMessageCommandHandler messageHandler:
                        _appCommandService.AddMessageCommand(
                            new MessageCommandBuilder(
                                messageHandler.Name,
                                (ApplicationCommandContext context, RestMessage message) => messageHandler.CommandDelegate(context, message, VegaInstance)
                            )
                        );
                        break;
                    case IUserCommandHandler userHandler:
                        _appCommandService.AddUserCommand(
                            new UserCommandBuilder(
                                userHandler.Name,
                                (ApplicationCommandContext context, User user) => userHandler.CommandDelegate(context, user, VegaInstance)
                            )
                        );
                        break;
                }
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
