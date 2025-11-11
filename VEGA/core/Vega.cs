using Handlers;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEGA.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace VEGA.Core;

public class Vega
{
    // whole configuration
    public Configuration Configuration { get; set; }
    // Client is created during Initialize; use null-forgiving here and assign in configureGatewayClient
    public GatewayClient Client { get; set; } = null!;
    // Initialize the command service so the property is non-null after construction
    public ApplicationCommandService<ApplicationCommandContext> ApplicationCommandService { get; set; } = null!;

    # region Constructor and Initializing

    public Vega(Configuration config)
    {
        Configuration = config;
    }

    public async Task Initialize()
    {
        // Use the fluent GatewayClientBuilder to create and configure the client and command service
        Client = Configurators.GatewayClientBuilder
                    .Create(Configuration.BotToken)
                    .Build();

        // Configure all registered handlers
        // Register all commands to Discord
        ApplicationCommandService = await Configurators.ApplicationCommandServiceBuilder
                                            .Create()
                                            .AddCommandHandlers(this)
                                            .BuildAsync(Client);

        ConfigureMiscHandlers();

        Client.InteractionCreate += async interaction =>
        {
            if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
                return;

            var result = await ApplicationCommandService.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, Client));

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
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    public void ConfigureMiscHandlers()
    {
        // Basically console logs
        Client.Connecting += async () => MiscHandlers.Connecting();
        Client.Connect += async () => MiscHandlers.Connected();
    }

    # endregion

    // Clear local ApplicationCommandService (remove all local commands)
    public void ClearLocalCommands()
    {
        // Recreate the service to remove registered commands and modules
        ApplicationCommandService = new ApplicationCommandService<ApplicationCommandContext>();
    }

    // Clear all commands on Discord (global or for a specific guild)
    public async Task ClearAllCommandsOnDiscordAsync(ulong? guildId = null)
    {
        var empty = Array.Empty<ApplicationCommandProperties>();
        if (guildId.HasValue)
            await Client.Rest.BulkOverwriteGuildApplicationCommandsAsync(Client.Id, guildId.Value, empty);
        else
            await Client.Rest.BulkOverwriteGlobalApplicationCommandsAsync(Client.Id, empty);
    }
}