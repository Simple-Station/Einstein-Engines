using Robust.Shared.Prototypes;

namespace Content.Shared.Mood;

[Prototype("moodEffect")]
public sealed class MoodEffectPrototype : IPrototype
{
    /// <summary>
    ///     The ID of the moodlet to use.
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The description of a moodlet in yml, used for player notifications.
    /// </summary>
    [DataField(required: true)]
    public string Description = string.Empty;

    /// <summary>
    ///     How much should this moodlet modify an entity's Mood.
    /// </summary>
    [DataField(required: true)]
    public float MoodChange;

    /// <summary>
    ///     How long, in Seconds, does this moodlet last? If omitted, the moodlet will last until canceled by any system.
    /// </summary>
    [DataField]
    public int Timeout;

    /// <summary>
    ///     Should this moodlet be hidden from the player? EG: No popups or chat messages.
    /// </summary>
    [DataField]
    public bool Hidden;

    /// <summary>
    ///     If mob already has effect of the same category, the new one will replace the old one.
    /// </summary>
    [DataField]
    public string? Category;
}
