namespace Content.Server.Speech.Components;

/// <summary>
///     Sends a random message from a list with a provided min/max time.
/// </summary>
[RegisterComponent]
public sealed partial class RandomBarkComponent : Component
{
    /// <summary>
    ///     Should the message be sent to the chat log?
    /// </summary>
    [DataField]
    public bool ChatLog = false;

    /// <summary>
    ///     Minimum time an animal will go without speaking
    /// </summary>
    [DataField]
    public int MinTime = 45;

    /// <summary>
    ///     Maximum time an animal will go without speaking
    /// </summary>
    [DataField]
    public int MaxTime = 350;

    /// <summary>
    ///     Accumulator for counting time since the last bark
    /// </summary>
    [DataField]
    public float BarkAccumulator = 8f;

    /// <summary>
    ///     Multiplier applied to the random time. Good for changing the frequency without having to specify exact values
    /// </summary>
    [DataField]
    public float BarkMultiplier = 1f;

    /// <summary>
    ///     Bark type, for use in locales. Locale keys follow the format "bark-{type}-{index between 1 and BarkLocaleCount}".
    /// </summary>
    [DataField]
    public string BarkType = "default";

    /// <summary>
    ///     Number of bark locales. If not specified, then it will be figured out by fetching the locale string
    ///     with the key "bark-{type}-count" and parsing it as an integer.
    /// </summary>
    [DataField]
    public int? BarkLocaleCount = null;
}
