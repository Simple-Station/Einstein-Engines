using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;

[RegisterComponent, NetworkedComponent]
public sealed partial class HatchingEggComponent : Component
{
    [DataField]
    public float CooldownTimer = 15.0f;

    [DataField]
    public EntityUid ShadowlingInside;

    public bool HasBeenHatched;

    // I know this looks dumb but it looks more readable and less error-prone than when I tried with an array
    // maybe there's a better way but its only 3 messages that need to appear
    public bool HasFirstMessageAppeared;
    public bool HasSecondMessageAppeared;
    public bool HasThirdMessageAppeared;
}
