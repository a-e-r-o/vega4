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
using Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public class Vega
{
    public DateTime StartTime { get; } = DateTime.UtcNow;
    // Client is created during Initialize; use null-forgiving here and assign in configureGatewayClient
    private ShardedGatewayClient ShardedClient { get; set; } = null!;
    // Initialize the command service so the property is non-null after construction
    private ApplicationCommandService<ApplicationCommandContext> ApplicationCommandService { get; set; } = null!;

    # region Constructor and Initializing

    public Vega(){}

    public async Task Initialize(MessageCreateHandler msgCreateHandler, string botToken)
    {
        // Use the fluent GatewayClientBuilder to create and configure the client and command service
        ShardedClient = Configurators.ShardedGatewayClientBuilder
                                        .Create(botToken)
                                        .Build();

        // Configure all registered handlers
        // Register all commands to Discord
        ApplicationCommandService = await Configurators.ApplicationCommandServiceBuilder
                                                        .Create()
                                                        .AddCommandHandlers()
                                                        .BuildAsync(ShardedClient);

        // Misc Handlers (static)
        ShardedClient.Connecting += async (client) => MiscHandlers.Connecting(client);
        ShardedClient.Connect += async (client) => await MiscHandlers.Connected(client);

        // Message Create (singleton)
        ShardedClient.MessageCreate += async (client, message) => await msgCreateHandler.MessageCreate(client, message);

        // Interaction Create
        ShardedClient.InteractionCreate += async (client, interaction) =>
        {
            if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
                return;

            string? errorMsg = null;

            try
            {
                IExecutionResult result = await ApplicationCommandService.ExecuteAsync(
                    new ApplicationCommandContext(applicationCommandInteraction, client)
                );
            }
            // Business exception = expected error that can be shown to user
            catch (BusinessException bex)
            {
                errorMsg = bex.Message;
            }
            // Other exceptions = unexpected error that should be logged
            catch (Exception ex)
            {
                errorMsg = "Interaction failed : an internal server error occurred.";
            }

            // If nay exception occurred, send failure response
            if (errorMsg != null)
            {
                try
                {
                    await interaction.SendResponseAsync(
                        InteractionCallback.Message(errorMsg)
                    );
                }
                catch
                {
                    Console.WriteLine("Failed to send interaction failure response.");  
                }
            }
        };
    }

    public async Task Launch()
    {
        await ShardedClient.StartAsync();
        await Task.Delay(-1);
    }

    # endregion

    // Clear local ApplicationCommandService (remove all local commands)
    public void ClearLocalCommands()
    {
        // Recreate the service to remove registered commands and modules
        ApplicationCommandService = new ApplicationCommandService<ApplicationCommandContext>();
    }

    // Clear all commands on Discord (global or for a specific guild) and terminate program
    public async Task ClearAllRegisteredCommandsAsync(ulong? guildId = null)
    {
        var empty = Array.Empty<ApplicationCommandProperties>();
        if (guildId.HasValue)
            await ShardedClient.Rest.BulkOverwriteGuildApplicationCommandsAsync(ShardedClient.Id, guildId.Value, empty);
        else
            await ShardedClient.Rest.BulkOverwriteGlobalApplicationCommandsAsync(ShardedClient.Id, empty);

        // Terminate the bot after clearing commands
        Environment.Exit(0);
    }
}