using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class HatchingEggComponent : Component
{
    [DataField]
    public float CooldownTimer = 5.0f;

    [DataField]
    public EntityUid ShadowlingInside;

    public bool HasBeenHatched;
}
