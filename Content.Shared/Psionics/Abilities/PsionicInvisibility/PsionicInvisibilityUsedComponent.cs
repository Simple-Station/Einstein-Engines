using Robust.Shared.Audio;

namespace Content.Shared.Abilities.Psionics;

[RegisterComponent]
public sealed partial class PsionicInvisibilityUsedComponent : Component
{
    [DataField]
    public float StunTime = 4f;

    [DataField]
    public float DamageToStun = 5f;

    [DataField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/Psionics/wavy.ogg");

    [DataField]
    public SoundSpecifier EndSound = new SoundPathSpecifier("/Audio/Psionics/wavy.ogg");
}
