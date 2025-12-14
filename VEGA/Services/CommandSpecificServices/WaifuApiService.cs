using System.Text.Json;
using Exceptions;
using Microsoft.AspNetCore.WebUtilities;
using Models.CommandSpecificModels;

namespace Services.CommandSpecificServices;

public class WaifuApiService
{
    public async Task<List<string>> FetchImagesAsync(int count, string category)
    {
        IApiDefinition apiToFetch;

        // Get all API in which the type (SFW category) exists
        var correspondingApis = WaifuApiTypes.GetApisWithCategoryBySfwCategoryValue(category);

        // Decide on which API to call (decided by which one contained the correct category)
        if (correspondingApis.Count > 1)
        {
            // Choose at random between APIs of the list
            int apiIndex = Random.Shared.Next(0, correspondingApis.Count);
            apiToFetch = correspondingApis[apiIndex];
        }
        else 
            apiToFetch = correspondingApis.First() ?? throw new SlashCommandBusinessException("Invalid category selected");

        List<string> res = new();

        switch (apiToFetch)
        {
            case WaifuPicsApiReference:
                res = await FetchApiWaifuPicAsync(count, category);
                break;
            case WaifuImApiReference:
                res = await FetchApiWaifuImAsync(count, category);
                break;
            default:
                throw new SlashCommandBusinessException("Inimplemented API");
        }

        return res;
    }

    private async Task<List<string>> FetchApiWaifuPicAsync(int count, string category)
    {
        bool multiple = count > 1;
        using HttpClient client = new HttpClient();

        string baseUri = WaifuApiTypes.WaifuPics.GetBaseUri(multiple);
        string url = string.Format(baseUri, "sfw", category);
        
        List<string> results = new();

        if (multiple)
        {
            HttpResponseMessage response = await client.PostAsync(
                url,
                // The "exclude" field is required, even when empty
                new FormUrlEncodedContent(new Dictionary<string, string> {{"exclude", ""}})
            );
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            var items = JsonSerializer.Deserialize<WaifuPicsApiReference.MultiplePicsApiResponse>(json);

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
            var item = JsonSerializer.Deserialize<WaifuPicsApiReference.SinglePicResponse>(json);

            results.Add(item!.Url!);
        }
        
        return results;
    }

    private async Task<List<string>> FetchApiWaifuImAsync(int count, string category)
    {
        using HttpClient client = new HttpClient();

        var queryParams = new Dictionary<string, string?>
        {
            ["is_nsfw"] = false.ToString(),
            ["included_tags"] = category,
        };
        if (count > 1)
            queryParams.Add("limit", count.ToString());
        
        string fullUrl = QueryHelpers.AddQueryString(WaifuApiTypes.WaifuIm.GetBaseUri(), queryParams);

        List<string> results = new();

        HttpResponseMessage response = await client.GetAsync(fullUrl);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<WaifuImApiReference.MultiplePicApiResponse>(json);

        foreach (var image in items!.Images)
        {
            results.Add(image.Url);
        }
        
        return results;
    }

}