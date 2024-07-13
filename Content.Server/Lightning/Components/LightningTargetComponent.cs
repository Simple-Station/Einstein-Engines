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
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Weighting = 1f;

    /// <summary>
    /// Lightning has a number of bounces into neighboring targets.
    /// This number controls how many bounces the lightning bolt has left after hitting that target.
    /// At high values, the lightning will not travel farther than that entity.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightningResistance = 1; //by default, reduces the number of bounces from this target by 1
}
