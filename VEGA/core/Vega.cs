using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public class Vega
{
    public DateTime StartTime { get; } = DateTime.UtcNow;
    // whole configuration
    private VegaConfiguration Configuration { get; set; }
    // Client is created during Initialize; use null-forgiving here and assign in configureGatewayClient
    private ShardedGatewayClient ShardedClient { get; set; } = null!;
    // Initialize the command service so the property is non-null after construction
    private ApplicationCommandService<ApplicationCommandContext> ApplicationCommandService { get; set; } = null!;

    # region Constructor and Initializing

    public Vega(VegaConfiguration config)
    {
        Configuration = config;
    }

    public async Task Initialize()
    {
        // Use the fluent GatewayClientBuilder to create and configure the client and command service
        ShardedClient = Configurators.ShardedGatewayClientBuilder
                            .Create(Configuration.BotToken)
                            .Build();

        // Configure all registered handlers
        // Register all commands to Discord
        var provider = ServiceRegistry.ServiceProvider ?? throw new InvalidOperationException("ServiceProvider not available for building command modules");

        ApplicationCommandService = await Configurators.ApplicationCommandServiceBuilder
                                            .Create()
                                            .AddCommandHandlers()
                                            .BuildAsync(ShardedClient);

        ConfigureMiscHandlers();

        ShardedClient.InteractionCreate += async (client, interaction) =>
        {
            if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
                return;

            IExecutionResult result = await ApplicationCommandService.ExecuteAsync
            (
                new ApplicationCommandContext(applicationCommandInteraction, client)
            );

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
    }

    public async Task Launch()
    {
        await ShardedClient.StartAsync();
        await Task.Delay(-1);
    }

    public void ConfigureMiscHandlers()
    {
        // Basically console logs
        ShardedClient.Connecting += async (client) => MiscHandlers.Connecting(client);
        ShardedClient.Connect += async (client) => MiscHandlers.Connected(client);
    }

    # endregion

    // Clear local ApplicationCommandService (remove all local commands)
    public void ClearLocalCommands()
    {
        // Recreate the service to remove registered commands and modules
        ApplicationCommandService = new ApplicationCommandService<ApplicationCommandContext>();
    }

    // Clear all commands on Discord (global or for a specific guild)
    public async Task ClearAllRegisteredCommandsAsync(ulong? guildId = null)
    {
        var empty = Array.Empty<ApplicationCommandProperties>();
        if (guildId.HasValue)
            await ShardedClient.Rest.BulkOverwriteGuildApplicationCommandsAsync(ShardedClient.Id, guildId.Value, empty);
        else
            await ShardedClient.Rest.BulkOverwriteGlobalApplicationCommandsAsync(ShardedClient.Id, empty);
    }
}