using Robust.Shared.Audio;

namespace Content.Shared.Silicon.EmitBuzzWhileDamaged;

/// <summary>
///     This is used for controlling the cadence of the buzzing emitted by EmitBuzzOnCritSystem.
///     This component is used by mechanical species that can get to critical health.
/// </summary>
[RegisterComponent]
public sealed partial class EmitBuzzWhileDamagedComponent : Component
{
    [DataField]
    public TimeSpan BuzzPopupCooldown = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public TimeSpan LastBuzzPopupTime;

    [DataField]
    public float CycleDelay = 2.0f;

    [ViewVariables]
    public float AccumulatedFrametime;

    [DataField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("buzzes");
}
