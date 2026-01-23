namespace Content.Server._EE.Scribe;

[RegisterComponent]
public sealed partial class ScribeBookComponent : Component
{
    /// <summary>
    ///     The key to find the Webhook data by.
    ///     See the formatting of the <see cref="Content.Shared.CCVar.CCVars.DiscordScribeWebhooks">DiscordScribeWebhooks</see> cvar for more information.
    /// </summary>
    [DataField]
    public string WebhookKey = "";

    /// <summary>
    ///     The fluent format to be used in the Webhook's username.
    ///     Null for default name.
    /// </summary>
    [DataField]
    public string? NameFormat = null;

    /// <summary>
    ///     The fluent format to be used in the Webhook's content.
    ///     Null for no formatting.
    /// </summary>
    [DataField]
    public string? ContentFormat = null;

    /// <summary>
    ///     The footer to be provided. Can be a fluent format string but can also be raw.
    ///     Null for no footer.
    /// </summary>
    [DataField]
    public string? Footer = null;

    /// <summary>
    ///     The colour to be used with the Webhook, in decimal.
    ///     See <see href="https://birdie0.github.io/discord-webhooks-guide/structure/embed/color.html">this guide</see> for more info.
    /// </summary>
    [DataField]
    public int Color = 0;
}
