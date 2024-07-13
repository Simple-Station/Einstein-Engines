using Content.Server.Tesla.EntitySystems;
using Content.Shared.Explosion;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.Lightning.Components;

/// <summary>
/// This component allows the lightning system to select a given entity as the target of a lightning strike.
/// It also determines the priority of selecting this target, and the behavior of the explosion. Used for tesla.
/// </summary>
[RegisterComponent, Access(typeof(LightningSystem), typeof(LightningTargetSystem))]
public sealed partial class LightningTargetComponent : Component
{
    /// <summary>
    /// The probability weighting of being stuck by lightning, compared against other nearby lightning targets.
    /// </summary>
    [DataField]
    public float Weighting = 1f;

    /// <summary>
    /// Lightning has a number of bounces into neighboring targets.
    /// This number controls how many bounces the lightning bolt has left after hitting that target.
    /// At high values, the lightning will not travel farther than that entity.
    /// For the love of god, do not make this number negative.
    /// </summary>
    [DataField]
    public int LightningArcReduction = 0;

    /// <summary>
    /// Lightning has a charge that gauges its energy.
    /// This number subtracts residual charge after absorption.
    /// </summary>
    [DataField]
    public float LightningChargeReduction = 0f;

    /// <summary>
    /// Lightning has a charge that gauges its energy.
    /// This number multiplies residual charge after absorption.
    /// </summary>
    [DataField]
    public float LightningChargeMultiplier = 1f;
}
