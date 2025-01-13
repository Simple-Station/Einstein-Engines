using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
///     Requires that the player ensures glimmer remain above a specific amount.
/// </summary>
[RegisterComponent, Access(typeof(RaiseGlimmerConditionSystem))]
public sealed partial class RaiseGlimmerConditionComponent : Component
{
    [DataField]
    public float Target;
}
