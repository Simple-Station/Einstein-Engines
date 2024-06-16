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
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool ChatLog = false;

    /// <summary>
    ///     Minimum time an animal will go without speaking
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinTime = 45;

    /// <summary>
    ///     Maximum time an animal will go without speaking
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxTime = 350;

    /// <summary>
    ///     Accumulator for counting time since the last bark
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BarkAccumulator = 8f;

    /// <summary>
    ///     Multiplier applied to the random time. Good for changing the frequency without having to specify exact values
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BarkMultiplier = 1f;

    /// <summary>
    ///     List of things to be said. Filled with garbage to be modified by an accent, but can be specified in the .yml
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public IReadOnlyList<string> Barks = new[]
    {
        "Bark",
        "Boof",
        "Woofums",
        "Rawrl",
        "Eeeeeee",
        "Barkums",
        "Awooooooooooooooooooo awoo awoooo",
        "Grrrrrrrrrrrrrrrrrr",
        "Rarrwrarrwr",
        "Goddamn I love gold fish crackers",
        "Bork bork boof boof bork bork boof boof boof bork",
        "Bark",
        "Boof",
        "Woofums",
        "Rawrl",
        "Eeeeeee",
        "Barkums",
    };
}
