using Robust.Shared.GameStates;

namespace Content.Shared.Overlays;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SaturationScaleOverlayComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SaturationScale = 1f;

    /// <summary>
    ///     Modifies how quickly the saturation "fades in", normally at a rate of 1% per second times this multiplier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FadeInMultiplier = 0.1f;
}
