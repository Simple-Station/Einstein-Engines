using Robust.Shared.Audio;

namespace Content.Server.Construction.Components;

[RegisterComponent]
public sealed partial class PartExchangerComponent : Component
{
    /// <summary>
    /// How long it takes to exchange the parts
    /// </summary>
    [DataField]
    public float ExchangeDuration = 3;

    /// <summary>
    /// Whether or not the distance check is needed.
    /// Good for BRPED.
    /// </summary>
    /// <remarks>
    /// I fucking hate BRPED and if you ever add it
    /// i will personally kill your dog.
    /// </remarks>
    [DataField]
    public bool DoDistanceCheck = true;

    [DataField]
    public SoundSpecifier ExchangeSound = new SoundPathSpecifier("/Audio/Items/rped.ogg");

    public EntityUid? AudioStream;
}
