using Content.Shared._White.Bark;
using Robust.Shared.Audio;

namespace Content.Client._White.Bark;

[RegisterComponent]
public sealed partial class BarkSourceComponent : Component
{
    [DataField]
    public Queue<BarkData> Barks { get; set; } = new();

    [DataField]
    public SoundSpecifier ResolvedSound { get; set; }

    [ViewVariables]
    public BarkData? CurrentBark { get; set; }

    [ViewVariables]
    public float BarkTime { get; set; }
}
