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
    public class ApplicationCommandServiceBuilder
    {
        private ApplicationCommandService<ApplicationCommandContext> _commandService;

        private ApplicationCommandServiceBuilder()
        {
            _commandService = new ApplicationCommandService<ApplicationCommandContext>();
        }

        public static ApplicationCommandServiceBuilder Create()
        {
            return new ApplicationCommandServiceBuilder();    
        } 

        public ApplicationCommandService<ApplicationCommandContext> Build()
        {
            return _commandService;
        }
    }
}
