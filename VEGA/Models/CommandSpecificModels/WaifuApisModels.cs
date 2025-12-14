using System;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Models.CommandSpecificModels;

public static class WaifuApiTypes
{
    public static List<IApiDefinition> ApiDefinitions => [WaifuPics, WaifuIm];

    public static readonly IApiDefinition WaifuPics = new WaifuPicsApiReference();
    public static readonly IApiDefinition WaifuIm = new WaifuImApiReference();

    public static IEnumerable<ApplicationCommandOptionChoiceProperties> GetAllSfwCategoriesDistinct(){
        return ApiDefinitions.SelectMany(x => x.SfwCategories)
                             .GroupBy(x => x.Name)
                             .Select(g => g.First());
    }

    /// <summary>
    /// Discord interaction context only gives us the value, not the name of the slash command argument option selected.
    /// Some categories are shared by name between API definitions but with different values.
    /// This function returns the API definitions containing the option selected.
    /// Exemple : category named "Any" = "any" or "waifu". Interaction context "category" argument : "waifu". 
    /// We find the category name for this value, which is "Any", and then return all ApiDefinitions 
    /// containing a category named "Any"
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public static List<IApiDefinition> GetApisWithCategoryBySfwCategoryValue(string category)
    {
        string categoryName = ApiDefinitions.SelectMany(x => x.SfwCategories)
                                            .First(x => x.StringValue == category)
                                            .Name;

        return ApiDefinitions.Where(
            x => x.SfwCategories.Exists(y => y.Name == categoryName)
        ).ToList();
    }
}

public interface IApiDefinition
{
    public class SinglePicResponse{};
    public class MultiplePicsApiResponse{};

    public string GetBaseUri(bool multiple = false);
    
    public List<ApplicationCommandOptionChoiceProperties> SfwCategories { get; }
}

/// <summary>
/// Models and consts for waifu.pics.
/// Reference : https://waifu.pics/docs
/// </summary>
public class WaifuPicsApiReference : IApiDefinition
{
    public class SinglePicResponse{
        [JsonPropertyName("url")]
        public required string Url {get; set;}
    }

    public class MultiplePicsApiResponse{
        [JsonPropertyName("files")]
        public required IEnumerable<string> PicUrls {get; set;}
    }

    public string GetBaseUri(bool multiple = false){
        return multiple ? "https://api.waifu.pics/many/{0}/{1}" : "https://api.waifu.pics/{0}/{1}";
    }

    public List<ApplicationCommandOptionChoiceProperties> SfwCategories => [
        new("Any", "waifu"),
        new("Megumin", "megumin"),
        new("Neko", "neko"),
        new("Shinobu", "shinobu")
    ];
}


/// <summary>
/// Models and consts for waifu.im
/// Reference : https://docs.waifu.im
/// </summary>
public class WaifuImApiReference : IApiDefinition
{
    public class MultiplePicApiResponse
    {
        [JsonPropertyName("images")]
        public required IEnumerable<ResponseImageItem> Images {get; set;}
    }
    public class ResponseImageItem
    {
        [JsonPropertyName("url")]
        public required string Url {get; set;}
    }

    public string GetBaseUri(bool multiple = false){
        return "https://api.waifu.im/search";
    }

    public List<ApplicationCommandOptionChoiceProperties> SfwCategories =>
    [
        new("Any", "waifu"),  // waifu
        new("Maid", "maid"),  // maid
        new("Marin Kitagawa", "marin-kitagawa"),  // marin-kitagawa
        new("Mori Calliope", "mori-calliope"),  // mori-calliope
        new("Raiden Shogun", "raiden-shogun"),  // raiden-shogun
        new("Uniform", "uniform"),  // uniform
        new("Kamisato Ayaka", "kamisato-ayaka"),  // kamisato-ayaka
    ];
}

public class SfwWaifuCategoryChoicesProvider : IChoicesProvider<ApplicationCommandContext>
{
    public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(SlashCommandParameter<ApplicationCommandContext> parameter)
    {
        // Group by name and select first element of grouping, so as to avoid showing 
        // the same category twice, in case of overlapping categories between different APIs
        return ValueTask.FromResult(
            WaifuApiTypes.GetAllSfwCategoriesDistinct()
        )!;
    }
}