using Exceptions;
using Microsoft.AspNetCore.Mvc.Routing;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace SlashCommands;

public class ShowProfile :  ApplicationCommandModule<ApplicationCommandContext>
{
    public const string SIZE_URL_PARAM = "?size=512";

    [SlashCommand("showprofile", "Show avatar and banner of a user in high res")]
    [RequireContext<ApplicationCommandContext>(RequiredContext.Guild)]
    public async Task Execute(
        [SlashCommandParameter(
            Name = "userid",
            Description = "Discord user ID"
        )]
        string strUserId
    )
    {
        if (!ulong.TryParse(strUserId, out ulong userId)) throw new SlashCommandBusinessException("Incorrect ID");

        // Defer message response before any API call
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        try
        {
            User user = await Context.Client.Rest.GetUserAsync(userId) ?? throw new SlashCommandBusinessException("User not found");

            ImageUrl? avatarUrl = user.GetAvatarUrl();
            ImageUrl? bannerUrl = user.GetBannerUrl();

            string res = $"{avatarUrl}{SIZE_URL_PARAM}";
            if(bannerUrl is not null)
                res += $"\n{bannerUrl}{SIZE_URL_PARAM}";


            var embed = new EmbedProperties
            {
                Title = user.GlobalName,
                Color = user.AccentColor ?? new Color(26, 28, 36),
                Fields = new[]
                {
                    new EmbedFieldProperties
                    {
                        Name = "Username",
                        Value = user.Username
                    },
                    new EmbedFieldProperties
                    {
                        Name = "Creation date",
                        Value = user.CreatedAt.ToString("dd/MM/yy")
                    },
                    new EmbedFieldProperties
                    {
                        Name = "User ID",
                        Value = $"`{user.Id}`"
                    }
                }
            };

            if(user.PrimaryGuild?.Tag is not null && user.PrimaryGuild.HasBadge)
            {
                string? badgeUrl = user.PrimaryGuild.GetBadgeUrl(ImageFormat.Png)?.ToString();

                var embedAuthor = new EmbedAuthorProperties {
                    Name = user.PrimaryGuild.Tag,
                };
                
                if (badgeUrl is not null)
                    embedAuthor.IconUrl = badgeUrl;

                embed.Author = embedAuthor;
            }

            if(bannerUrl is not null)
                embed.Image = $"\n{bannerUrl}{SIZE_URL_PARAM}";

            if(avatarUrl is not null)
                embed.Thumbnail = new EmbedThumbnailProperties($"{avatarUrl}{SIZE_URL_PARAM}");

            await Context.Interaction.SendResponseAsync(
                InteractionCallback.Message(
                    new InteractionMessageProperties
                    {
                        Embeds = new[] { embed }
                    }
                )
            );
        }
        catch (SlashCommandException ex)
        {
            ex.Deferred = true;
            throw;
        }
    }
}