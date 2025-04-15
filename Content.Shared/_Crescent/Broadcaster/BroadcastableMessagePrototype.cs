using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Crescent.Broadcaster;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("broadcastMessage")]
public sealed partial class BroadcastableMessagePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Name that shows in the Broadcasting Console
    /// </summary>
    [DataField]
    public string Name = default!;

    [DataField("sound")]
    public SoundPathSpecifier announceSound = default!;

    /// <summary>
    ///  Identifier for letting it know what can play it and what can't
    /// </summary>
    [DataField("outpost")]
    public string Outpost = default!;

    [DataField("text")]
    public string Text = default!;
}
