using Core;
using Exceptions;
using MessageCommands;
using Microsoft.Extensions.DependencyInjection;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Resources;
using static Core.GlobalRegistry;

namespace ComponentCommands;

/// <summary>
/// Routes button clicks emitted by the /DownloadEmotes widget.
/// The customId carries the widgetId that points to the cached emote list;
/// NetCord's ComponentInteractionService binds it as the action's parameter.
/// </summary>
public class EmoteWidgetButtons : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(DownloadEmotes.CUSTOMID_ZIP_PREFIX)]
    public async Task ZipAsync(string widgetId)
    {
        // Public response (the zip is posted in the channel).
        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredMessage());

        var state = DownloadEmotes.LoadWidgetState(widgetId);
        DownloadEmotes.EnsureInvoker(Context.Interaction, state);

        var httpClient = MainServiceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientNames.Emotes);

        var downloaded = await DownloadEmotes.DownloadEmotesAsync(state.Emotes, httpClient);

        using var zipStream = await DownloadEmotes.BuildZipAsync(downloaded);

        var locale = Context.Interaction.UserLocale;
        await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
        {
            Content = ResourceHelper.GetString(
                state.Emotes.Count > 1 ? Strings.Commands.DlEmotesResultMultiple : Strings.Commands.DlEmotesResultSingle,
                locale,
                state.Emotes.Count
            ),
            Attachments = new[]
            {
                new AttachmentProperties(DownloadEmotes.EMOTE_ZIPFILE_NAME, zipStream)
            }
        });
    }

    [ComponentInteraction(DownloadEmotes.CUSTOMID_ADD_PREFIX)]
    public async Task AddAsync(string widgetId)
    {
        // Ephemeral response (only the invoker sees the outcome).
        await Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        if (!Context.Interaction.GuildId.HasValue)
            throw new SlashCommandBusinessException(Strings.Exceptions.DlEmotesNotInGuild);

        var state = DownloadEmotes.LoadWidgetState(widgetId);
        DownloadEmotes.EnsureInvoker(Context.Interaction, state);

        // Fail-closed: anything that isn't a guild member holding the emoji-manage
        // permission is rejected. A non-GuildInteractionUser context would otherwise
        // slip through the previous `&&` chain without any check being applied.
        if (Context.Interaction.User is not GuildInteractionUser guildUser
            || (!guildUser.Permissions.HasFlag(Permissions.CreateGuildExpressions)
                && !guildUser.Permissions.HasFlag(Permissions.ManageGuildExpressions)))
        {
            throw new SlashCommandBusinessException(Strings.Exceptions.DlEmotesUserMissingEmojiPerm);
        }

        if (!Context.Interaction.AppPermissions.HasFlag(Permissions.CreateGuildExpressions)
            && !Context.Interaction.AppPermissions.HasFlag(Permissions.ManageGuildExpressions))
        {
            throw new SlashCommandBusinessException(Strings.Exceptions.DlEmotesBotMissingEmojiPerm);
        }

        var httpClient = MainServiceProvider
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient(HttpClientNames.Emotes);

        // Bound the whole download+upload batch. Emoji creation is heavily rate-limited,
        // so a large batch (or a route hanging past its budget) would otherwise leave the
        // interaction "thinking" until the token silently expires — no feedback for the user.
        // On timeout we still report how many emotes made it through before aborting.
        using var cts = new CancellationTokenSource(
            TimeSpan.FromSeconds(DownloadEmotes.ADD_EMOTES_TIMEOUT_SECONDS));

        var locale = Context.Interaction.UserLocale;
        int created;
        List<string> failures;
        bool timedOut;

        try
        {
            var downloaded = await DownloadEmotes.DownloadEmotesAsync(state.Emotes, httpClient, cts.Token);

            var restClient = MainServiceProvider.GetRequiredService<Vega>().Rest;
            (created, failures, timedOut) = await DownloadEmotes.AddEmotesToGuildAsync(
                Context.Interaction.GuildId.Value, downloaded, restClient, cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Timeout during the download phase, before any emoji was created.
            created = 0;
            failures = new List<string>();
            timedOut = true;
        }

        string content;
        if (timedOut)
        {
            content = ResourceHelper.GetString(Strings.Commands.DlEmotesTimeout, locale, created);
        }
        else if (failures.Count == 0)
        {
            content = ResourceHelper.GetString(Strings.Commands.DlEmotesAddedAll, locale, created);
        }
        else
        {
            content = ResourceHelper.GetString(
                Strings.Commands.DlEmotesAddedPartial,
                locale,
                created,
                failures.Count,
                string.Join("\n", failures.Select(f => "• " + f))
            );
        }

        await Context.Interaction.SendFollowupMessageAsync(new InteractionMessageProperties
        {
            Content = content,
            Flags = MessageFlags.Ephemeral
        });
    }
}
