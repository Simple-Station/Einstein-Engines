namespace Content.Server.Speech.Components;

/// <summary>
///     Sends a random message from a list with a provided min/max time.
/// </summary>
[RegisterComponent]
public sealed partial class RandomBarkComponent : Component
{
    // Should the message be sent to the chat log?
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool ChatLog = false;

    // Minimum time an animal will go without speaking
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MinTime = 45;

    // Maximum time an animal will go without speaking
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int MaxTime = 350;

    // Counter
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BarkAccumulator = 8f;

    // Multiplier applied to the random time. Good for changing the frequency without having to specify exact values
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float BarkMultiplier = 1f;

    // List of things to be said. Filled with garbage to be modified by an accent, but can be specified in the .yml
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
