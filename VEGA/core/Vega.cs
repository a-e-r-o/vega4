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
using Exceptions;

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
            bool deferred = false;
            
            try
            {
                IExecutionResult result = await ApplicationCommandService.ExecuteAsync(
                    new ApplicationCommandContext(applicationCommandInteraction, client)
                ).ConfigureAwait(false);

                // Case of exception thrown : rethrow the exception wrapped in the execution result
                if (result is ExecutionExceptionResult executionExceptionResult)
                    throw executionExceptionResult.Exception; 

                // Case of missing perm : wrap MissingPerm object into a custom exception and throw it
                if (result is MissingPermissionsResult missingPerm)
                {
                    await interaction.SendResponseAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
                    deferred = true;
                    throw new MissingPermissionException(missingPerm);
                }
            }
            catch (MissingPermissionException pex)
            {   
                var missingPerms = Enum.GetValues<Permissions>()
                                       .Cast<Permissions>()
                                       .Where(flag => pex.MissingPerm.MissingPermissions.HasFlag(flag))
                                       .Select(flag => flag.ToString());

                string strMissingPerms = string.Join(",", missingPerms);

                switch (pex.MissingPerm.EntityType)
                {
                    case MissingPermissionsResultEntityType.Bot: 
                        errorMsg = $"Can't execute command. Bot is missing permissions : {strMissingPerms}";
                        break;
                    case MissingPermissionsResultEntityType.User:
                        errorMsg = $"You can't use this command. Missing permissions : {strMissingPerms}";
                        break;
                }
            }
            // Expected exception with user-readable message
            catch (SlashCommandBusinessException bex)
            {
                errorMsg = bex.Message;
                deferred = bex.Deferred;
            }
            // Unexpected, caught exception
            catch (SlashCommandGenericException gex)
            {
                errorMsg = "Something went wrong while executing command ¯\\\\_(ツ)\\_/¯";
                deferred = gex.Deferred;
            }
            // Worst case scenario : unexcepted uncaught exception
            catch (Exception ex)
            {
                errorMsg = "Something went terribly wrong while executing command. Ask the dev to fix their broken code";
            }

            // If nay exception occurred, send failure response
            if (errorMsg != null)
            {
                try
                {
                    // Reply in followup response to the deferred message
                    if (deferred)
                    {
                        await interaction.SendFollowupMessageAsync(
                            errorMsg
                        );
                    }
                    // Reply to interaction
                    else
                    {   
                        var elapsed = DateTimeOffset.UtcNow - interaction.CreatedAt;

                        // Check if interaction can still be responded to directly
                        // If not, log and do nothing
                        if (elapsed.TotalSeconds > 2.5)
                        {
                            Console.WriteLine("Interaction took more than 2 (1 sec margin) sec to process).");
                        }
                        else
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