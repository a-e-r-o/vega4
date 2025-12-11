using System.Text.Json;
using System.Text.Json.Serialization;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

public class WaifuCategoryChoicesProvider : IChoicesProvider<ApplicationCommandContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(SlashCommandParameter<ApplicationCommandContext> parameter)
    {
        var list = new List<ApplicationCommandOptionChoiceProperties>
        {
            new("Any", "waifu"),
            new("Megumin", "megumin"),
            new("Neko", "neko"),
            new("Shinobu", "shinobu")
        }.AsEnumerable();
        
        return ValueTask.FromResult(list);
    }
}

public class SingleWaifuApiResponse{
    [JsonPropertyName("url")]
    public string? Url {get; set;}
}

public class MultipleWaifuApiResponse{
    [JsonPropertyName("files")]
    public IEnumerable<string>? PicUrls {get; set;}
}
 
public class GetWaifu : ApplicationCommandModule<ApplicationCommandContext>
{
    private const string waifuApiSingleBaseUri = "https://api.waifu.pics/{0}/{1}";
    private const string waifuApiManyBaseUri = "https://api.waifu.pics/many/{0}/{1}";

    [RequireUserPermissions<ApplicationCommandContext>(Permissions.AttachFiles)]
    [RequireBotPermissions<ApplicationCommandContext>(Permissions.AttachFiles)]
    [SlashCommand("waifu", "Send a waifu image")]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "type", 
            Description = "Type of waifu to send",
            ChoicesProviderType = typeof(WaifuCategoryChoicesProvider)
        )] string type = "waifu",
        [SlashCommandParameter(
            Name = "count", Description = "Number of waifu to send", MinValue = 1, MaxValue = 5
        )] int count = 1
    )
    {
        bool multiple = count > 1;
        string baseUri = multiple ? waifuApiManyBaseUri : waifuApiSingleBaseUri;
        string url = string.Format(baseUri, "sfw", type);
        
        List<string> results = new();

        using HttpClient client = new HttpClient();

        try
        {
            if (multiple)
            {
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    // The "exclude" field is required, even when empty
                    new FormUrlEncodedContent(new Dictionary<string, string> {{"exclude", ""}})
                );
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var items = JsonSerializer.Deserialize<MultipleWaifuApiResponse>(json);

                foreach (var picUrl in items!.PicUrls!.Take(count))
                {
                    results.Add(picUrl);
                }
            }
            else
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync();
                var item = JsonSerializer.Deserialize<SingleWaifuApiResponse>(json);

                results.Add(item!.Url!);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur inattendue: {ex.Message}");
        }

        await Context.Interaction.SendResponseAsync(
            InteractionCallback.Message(
                string.Join("\n", results)
            )
        );
    }
}