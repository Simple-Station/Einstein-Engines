using Robust.Shared.Audio;

namespace Content.Shared.Silicon.DeadStartupButton;

/// <summary>
///     This is used for Silicon entities such as IPCs, Cyborgs, Androids, anything "living" with a button people can touch.
/// </summary>
[RegisterComponent]
public sealed partial class DeadStartupButtonComponent : Component
{
    [DataField]
    public string VerbText = "dead-startup-button-verb";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/Arcade/newgame.ogg");

    [DataField]
    public SoundSpecifier ButtonSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    [DataField]
    public float DoAfterInterval = 1f;

    [DataField]
    public SoundSpecifier BuzzSound = new SoundCollectionSpecifier("buzzes");

    [DataField]
    public int VerbPriority = 1;
}
