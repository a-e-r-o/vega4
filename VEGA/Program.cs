using Microsoft.Extensions.Configuration;
using NetCord;
using NetCord.Services.ApplicationCommands;
using Microsoft.Extensions.Hosting;
using NetCord.Hosting.Gateway;
using NetCord.Gateway;
using NetCord.Logging;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Rest;
using NetCord.Services;
using System.Threading.Tasks;


// Configuration
var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
IConfiguration configuration = builder.Build();

Configuration config = new Configuration();
config.BotToken = configuration.GetValue<string>("botToken") ?? throw new Exception("token not found");

Vega bot = new(config);
await bot.Initialize();
await bot.Launch();

public class Vega
{
    private Configuration Configuration { get; set; }
    // Client is created during Initialize; use null-forgiving here and assign in configureGatewayClient
    public GatewayClient Client { get; set; } = null!;
    // Initialize the command service so the property is non-null after construction
    public ApplicationCommandService<ApplicationCommandContext> ApplicationCommandService { get; set; } = new();

    public Vega(Configuration config){
        Configuration = config;
    }

    public async Task Initialize()
    {
        // Create the gateway client first, then attach handlers and configure commands
        configureGatewayClient(Configuration.BotToken);
        ConfigureHandlers();
        ConfigureApplicationCommandService();
        await RegisterCommands();
    }

    public async Task Launch()
    {
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    public async Task RegisterCommands()
    {
        // Register the commands so that you can use them in the Discord client
        await ApplicationCommandService.RegisterCommandsAsync(Client.Rest, Client.Id);
    }

    private void configureGatewayClient(string token)
    {
        // Creating client
        Client = new(new BotToken(token), new GatewayClientConfiguration
        {
            Intents = GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent,
            Logger = new ConsoleLogger(),
        });
    }

    private void ConfigureApplicationCommandService(){
        ApplicationCommandService = new();

        // Add commands using minimal APIs
        ApplicationCommandService.AddSlashCommand(new SlashCommandBuilder("ping", "Ping!", () => "Pong!"));
        ApplicationCommandService.AddUserCommand(new UserCommandBuilder("Username", (User user) => user.Username));
        ApplicationCommandService.AddMessageCommand(new MessageCommandBuilder("Length", (RestMessage message) => message.Content.Length.ToString()));

        // Add commands from modules
        ApplicationCommandService.AddModules(typeof(Program).Assembly);
    }

    private void ConfigureHandlers()
    {
        Client.Connect += async () =>
        {
            Console.WriteLine("connected");
        };

        Client.Connecting += async () =>
        {
            Console.WriteLine("connecting");
        };

        // Add the handler to handle interactions
        Client.InteractionCreate += async interaction =>
        {
            // Check if the interaction is an application command interaction
            if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
                return;

            // Execute the command
            var result = await ApplicationCommandService.ExecuteAsync(new ApplicationCommandContext(applicationCommandInteraction, Client));

            // Check if the execution failed
            if (result is not IFailResult failResult)
                return;

            // Return the error message to the user if the execution failed
            try
            {
                await interaction.SendResponseAsync(InteractionCallback.Message(failResult.Message));
            }
            catch
            {
            }
        };
    }
    
    async Task HandleInteraction (Interaction interaction)
    {
        if (interaction is not ApplicationCommandInteraction applicationCommandInteraction)
            return;
    }
}
