using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;
using VEGA.Core.Models;

namespace VEGA.Core;


public class Vega
{
    // whole configuration
    private Configuration Configuration { get; set; }
    // Client is created during Initialize; use null-forgiving here and assign in configureGatewayClient
    public GatewayClient Client { get; set; } = null!;
    // Initialize the command service so the property is non-null after construction
    public ApplicationCommandService<ApplicationCommandContext> ApplicationCommandService { get; set; } = new();

    public Vega(Configuration config)
    {
        Configuration = config;
    }

    public async Task Initialize()
    {
        // Use the fluent GatewayClientBuilder to create and configure the client and command service
        var built = await Configurators.GatewayClientBuilder
            .Create(Configuration.BotToken)
            .ConfigureCommands(typeof(Vega).Assembly)
            .RegisterHandlers()
            .BuildAsync();

        Client = built.client;
        ApplicationCommandService = built.service;
    }

    public async Task Launch()
    {
        await Client.StartAsync();
        await Task.Delay(-1);
    }
}
