using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlasmaVesselComponent : Component
{
    /// <summary>
    /// The total amount of plasma the alien has.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Plasma = 0;

    /// <summary>
    /// The entity's current max amount of essence. Can be increased
    /// through harvesting player souls.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxPlasma")]
    public FixedPoint2 PlasmaRegenCap = 500;

    [ViewVariables]
    public FixedPoint2 PlasmaPerSecond = 1f;

    /// <summary>
    /// The amount of plasma passively generated per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("plasmaPerSecond")]
    public FixedPoint2 PlasmaUnmodified = 1f;

    [ViewVariables]
    public float Accumulator = 0;

    /// <summary>
    /// The amount of plasma to which plasma per second will be equal, when alien stands on weeds
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("plasmaModified")]
    public float WeedModifier = 1.5f;
}
