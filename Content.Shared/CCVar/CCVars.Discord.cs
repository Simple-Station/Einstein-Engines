using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// The role that will get mentioned if a new SOS ahelp comes in.
    /// </summary>
    public static readonly CVarDef<string> DiscordAhelpMention =
        CVarDef.Create("discord.on_call_ping", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// URL of the discord webhook to relay unanswered ahelp messages.
    /// </summary>
    public static readonly CVarDef<string> DiscordOnCallWebhook =
        CVarDef.Create("discord.on_call_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// URL of the Discord webhook which will relay all ahelp messages.
    /// </summary>
    public static readonly CVarDef<string> DiscordAHelpWebhook =
        CVarDef.Create("discord.ahelp_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// The server icon to use in the Discord ahelp embed footer.
    /// Valid values are specified at https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure.
    /// </summary>
    public static readonly CVarDef<string> DiscordAHelpFooterIcon =
        CVarDef.Create("discord.ahelp_footer_icon", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// The avatar to use for the webhook. Should be an URL.
    /// </summary>
    public static readonly CVarDef<string> DiscordAHelpAvatar =
        CVarDef.Create("discord.ahelp_avatar", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// URL of the Discord webhook which will relay all custom votes. If left empty, disables the webhook.
    /// </summary>
    public static readonly CVarDef<string> DiscordVoteWebhook =
        CVarDef.Create("discord.vote_webhook", string.Empty, CVar.SERVERONLY);

    /// URL of the Discord webhook which will relay round restart messages.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundUpdateWebhook =
        CVarDef.Create("discord.round_update_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Role id for the Discord webhook to ping when the round ends.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundEndRoleWebhook =
        CVarDef.Create("discord.round_end_role", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Enable Discord linking, show linking button and modal window
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("discord.auth_enabled", false, CVar.SERVERONLY);

    /// <summary>
    ///     URL of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiUrl =
        CVarDef.Create("discord.auth_api_url", "", CVar.SERVERONLY);

    /// <summary>
    ///     Secret key of the Discord auth server API
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthApiKey =
        CVarDef.Create("discord.auth_api_key", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);
}
