using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using NetCord.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace Core;

public class Vega
{
    // Client is created during Initialize; use null-forgiving here and assign in configureGatewayClient
    private ShardedGatewayClient ShardedClient { get; set; } = null!;
    // Initialize the command service so the property is non-null after construction
    private ApplicationCommandService<ApplicationCommandContext> ApplicationCommandService { get; set; } = null!;

    # region Constructor and Initializing

    public Vega(){}

    public async Task Initialize(MessageCreateHandler msgCreateHandler, string botToken)
    {
        // Use the fluent GatewayClientBuilder to create and configure the client and command service
        ShardedClient = new ShardedGatewayClient
        (
            new BotToken(botToken), new ShardedGatewayClientConfiguration
            {
                IntentsFactory = (shard) => GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent | GatewayIntents.Guilds | GatewayIntents.GuildUsers,
                LoggerFactory = ShardedConsoleLogger.GetFactory()
            }
        );

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
            bool isBusinessEx = false;

            try
            {
                IExecutionResult result = await ApplicationCommandService.ExecuteAsync(
                    new ApplicationCommandContext(applicationCommandInteraction, client)
                ).ConfigureAwait(false);

                if (result is ExecutionExceptionResult executionExceptionResult)
                {
                    throw executionExceptionResult.Exception; // Rethrow the wrapped exception
                }
            }
            // Business exception = expected error that can be shown to user
            catch (BusinessException bex)
            {
                errorMsg = bex.Message;
                isBusinessEx = true;
            }
            // Other exceptions = unexpected error that should be logged
            catch (Exception ex)
            {
                errorMsg = "Something went wrong in the server. Ask the dev to fix their broken code";
            }

            // If nay exception occurred, send failure response
            if (errorMsg != null)
            {
                try
                {
                    // Reply as interaction response with ephemal flag (user-only)
                    if (isBusinessEx)
                    {
                        await interaction.SendResponseAsync(
                            InteractionCallback.Message(
                                new InteractionMessageProperties
                                {
                                    Content = errorMsg,
                                    Flags = MessageFlags.Ephemeral
                                }
                            )
                        );
                    }
                    // Reply as a simple message in channel
                    else
                    {   
                        await interaction.Channel.SendMessageAsync(
                            $"Interaction failed : {errorMsg}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to send interaction failure response.", ex.Message);
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