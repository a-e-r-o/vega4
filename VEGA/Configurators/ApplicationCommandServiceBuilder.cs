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
            // Creating lists of handler instances
            var msgCommandHandlers = new List<IMessageCommandHandler>();
            var usrCommandHandlers = new List<IUserCommandHandler>();
            var slhCommandHandlers = new List<SlashCommandBase>();

            // Discovery and creating instances of handler classes
            var msgHandlerClasses = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "Handlers"
                        && t.IsClass
                        && !t.IsAbstract
                        && typeof(IMessageCommandHandler).IsAssignableFrom(t));

            var userHandlerClasses = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "Handlers"
                        && t.IsClass
                        && !t.IsAbstract
                        && typeof(IUserCommandHandler).IsAssignableFrom(t));

            var slhHandlerClasses = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Namespace == "Handlers"
                        && t.IsClass
                        && !t.IsAbstract
                        && typeof(SlashCommandBase).IsAssignableFrom(t));

                        
            // Creating instances and populating instance lists
            foreach (Type type in slhHandlerClasses.Concat(userHandlerClasses).Concat(msgHandlerClasses))
            {

                Console.WriteLine($"Creating instance of handler type : {type.Name}");

                switch (type)
                {
                    // User command
                    case Type t when t == typeof(IUserCommandHandler):
                        var usrHandler = Activator.CreateInstance(type) as IUserCommandHandler;
                        usrCommandHandlers.Add(usrHandler!);
                        break;
                    // Message command
                    case Type t when t == typeof(IMessageCommandHandler):
                        var msgHandler = Activator.CreateInstance(type) as IMessageCommandHandler;
                        msgCommandHandlers.Add(msgHandler!);
                        break;
                    // Slash command
                    case Type t when t.BaseType == typeof(SlashCommandBase):
                        var instance = Activator.CreateInstance(type, VegaInstance) as SlashCommandBase;
                        slhCommandHandlers.Add(instance!);
                        break;
                    
                    default:
                        throw new NotSupportedException($"Type {type.Name} not supported");
                }
            }

            // Adding UserHandlers to service
            foreach(IUserCommandHandler usrHandler in usrCommandHandlers){
                _appCommandService.AddUserCommand
                (
                    new UserCommandBuilder
                    (
                        usrHandler.Name,
                        (ApplicationCommandContext context, User user) => usrHandler.Execute(context, user, VegaInstance)
                    )
                );
            }

            // Adding MsgHandlers to service
            foreach(IMessageCommandHandler msgHandler in msgCommandHandlers){
                _appCommandService.AddMessageCommand
                (
                    new MessageCommandBuilder
                    (
                        msgHandler.Name,
                        (ApplicationCommandContext context, RestMessage message) => msgHandler.Execute(context, message, VegaInstance)
                    )
                );
            }

            // Collect group builders for any sub-commands we need to create
            var groupBuilders = new Dictionary<string, SlashCommandGroupBuilder>();

            // Adding SlashCommands and sub-SlashCommands to service
            foreach (var slhHandler in slhCommandHandlers)
            {
                
                _appCommandService.AddSlashCommand
                (
                    new SlashCommandBuilder
                    (
                        slhHandler.Name,
                        slhHandler.Description,
                        slhHandler.CommandDelegate
                    )
                );

                /*
                // Grouping by using a name with a space: "group subcommand"
                var parts = handler.Name.Split(new[] { ' ' }, 2); // Todo 
                if (parts.Length == 2)
                {
                    var groupName = parts[0];
                    var subName = parts[1];
                    if (!groupBuilders.TryGetValue(groupName, out var group))
                    {
                        group = new SlashCommandGroupBuilder(groupName, groupName);
                        groupBuilders[groupName] = group;
                    }
                    group.AddSubCommand
                    (
                        subName,
                        handler.Description,
                        (ApplicationCommandContext context) => handler.CommandDelegate(context, VegaInstance)
                    );
                }
                else
                {
                    // No group specified, add as a top-level slash command with a parameter
                }
                */

            }

            // Register any created slash command groups
            foreach (var gb in groupBuilders.Values)
            {
                _appCommandService.AddSlashCommandGroup(gb);
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
