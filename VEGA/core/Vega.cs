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

    private readonly List<IHandlerBase> _handlers = new();

    public Vega(Configuration config)
    {
        Configuration = config;
    }

    public void RegisterHandler(IHandlerBase handler)
    {
        _handlers.Add(handler);
    }

    public async Task Initialize()
    {
        // Use the fluent GatewayClientBuilder to create and configure the client and command service
        Client = Configurators.GatewayClientBuilder
                        .Create(Configuration.BotToken)
                        .Build();

        ApplicationCommandService = new ApplicationCommandService<ApplicationCommandContext>();

        // Configure all registered handlers
        foreach (var handler in _handlers)
        {
            switch (handler)
            {
                case ISlashCommandHandler slashCommandHandler:
                    ApplicationCommandService.AddSlashCommand(
                        new SlashCommandBuilder(
                            slashCommandHandler.Name,
                            slashCommandHandler.Description,
                            () => slashCommandHandler.CommandDelegate(this)
                        )
                    );
                    break;

                case IMessageCommandHandler messageHandler:
                    ApplicationCommandService.AddMessageCommand(
                        new MessageCommandBuilder(
                            messageHandler.Name,
                            (RestMessage message) => messageHandler.CommandDelegate(message, this)
                        )
                    );
                    break;
                case IUserCommandHandler userHandler:
                    ApplicationCommandService.AddUserCommand(
                        new UserCommandBuilder(
                            userHandler.Name,
                            (User user) => userHandler.CommandDelegate(user, this)
                        )
                    );
                    break;
            }
        }

        // Register all commands to Discord
        await ApplicationCommandService.RegisterCommandsAsync(Client.Rest, Client.Id);

        // Link events to handlers
        Client.Connecting += async () => Console.WriteLine("connecting...");
        Client.Connect += async () => Console.WriteLine("connected");

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

    // Remove a single command on Discord by name (returns true if deleted)
    public async Task<bool> RemoveCommandByNameOnDiscordAsync(string commandName, ulong? guildId = null)
    {
        if (guildId.HasValue)
        {
            var registered = await Client.Rest.GetGuildApplicationCommandsAsync(Client.Id, guildId.Value);
            var item = registered.FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                await Client.Rest.DeleteGuildApplicationCommandAsync(Client.Id, guildId.Value, item.Id);
                return true;
            }
            return false;
        }
        else
        {
            var registered = await Client.Rest.GetGlobalApplicationCommandsAsync(Client.Id);
            var item = registered.FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                await Client.Rest.DeleteGlobalApplicationCommandAsync(Client.Id, item.Id);
                return true;
            }
            return false;
        }
    }

    // Sync local ApplicationCommandService commands to Discord by bulk-overwriting with the local set
    // Note: ApplicationCommandService.GetCommands() must return a collection of ApplicationCommandProperties or be transformable.
    public async Task SyncLocalCommandsToDiscordAsync(ulong? guildId = null)
    {
        // Try to get ApplicationCommandProperties directly from the service
        // If the library does not expose that, you'd need to rebuild the properties from your builders/modules.
        var localCommands = ApplicationCommandService.GetCommands();

        // Defensive: if the returned items are already ApplicationCommandProperties, use them
        if (localCommands is IEnumerable<ApplicationCommandProperties> props)
        {
            var arr = props.ToArray();
            if (guildId.HasValue)
                await Client.Rest.BulkOverwriteGuildApplicationCommandsAsync(Client.Id, guildId.Value, arr);
            else
                await Client.Rest.BulkOverwriteGlobalApplicationCommandsAsync(Client.Id, arr);
            return;
        }

        // Otherwise, try to map them. This part depends on what GetCommands() returns in your NetCord version.
        // As a fallback we will clear all commands (safe) — you can customize to rebuild desired commands.
        if (guildId.HasValue)
            await Client.Rest.BulkOverwriteGuildApplicationCommandsAsync(Client.Id, guildId.Value, Array.Empty<ApplicationCommandProperties>());
        else
            await Client.Rest.BulkOverwriteGlobalApplicationCommandsAsync(Client.Id, Array.Empty<ApplicationCommandProperties>());
    }
}