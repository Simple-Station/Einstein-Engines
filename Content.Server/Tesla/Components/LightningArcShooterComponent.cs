using Content.Server.Tesla.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server.Tesla.Components;

/// <summary>
/// Periodically fires electric arcs at surrounding objects.
/// </summary>
[RegisterComponent, Access(typeof(LightningArcShooterSystem)), AutoGenerateComponentPause]
public sealed partial class LightningArcShooterComponent : Component
{
    /// <summary>
    /// The total number of arcs that an energy ball can create from any one shot
    /// Increasing this value can lead to increasingly unpredictable ranges
    [DataField]
    public int MaxArcs = 4;

    /// <summary>
    /// The target selection radius for a lightning bolt 'arc' bounce/chain.
    /// </summary>
    [DataField]
    public float ArcRadius = 3.5f;

    /// <summary>
    /// The number of lightning bolts that are fired at the same time. From 0 to N
    /// Important balance value: if there aren't a N number of coils or grounders around the tesla,
    /// the tesla will have a chance to shoot into something important and break.
    /// </summary>
    [DataField]
    public int MaxBolts = 1;

    /// <summary>
    /// The target selection radius for lightning bolts.
    /// </summary>
    [DataField]
    public float BoltRadius = 3.5f;

    /// <summary>
    /// The chance that any given lightning arc will fork instead of chain
    /// </summary>
    [DataField]
    public float ForkChance = 0.5f;

    /// <summary>
    /// The max number of forks for any given lighting arc
    /// Must be 2 or higher
    /// </summary>
    [DataField]
    public int MaxForks = 2;


    /// <summary>
    /// Minimum interval between shooting.
    /// </summary>
    [DataField]
    public float ShootMinInterval = 0.5f;

    /// <summary>
    /// Maximum interval between shooting.
    /// </summary>
    [DataField]
    public float ShootMaxInterval = 8.0f;

    /// <summary>
    /// The time, upon reaching which the next batch of lightning bolts will be fired.
    /// </summary>
    [DataField]
    [AutoPausedField]
    public TimeSpan NextShootTime;

    /// <summary>
    /// The type of lightning bolts we shoot
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId LightningPrototype = "Lightning";
}
