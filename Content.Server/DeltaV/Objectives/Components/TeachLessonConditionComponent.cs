using Content.Server.DeltaV.Objectives.Systems;
using Content.Server.Objectives.Components;

namespace Content.Server.DeltaV.Objectives.Components;

/// <summary>
/// Requires that a target dies once and only once.
/// Depends on <see cref="TargetObjectiveComponent"/> to function.
/// </summary>
[RegisterComponent, Access(typeof(TeachLessonConditionSystem))]
public sealed partial class TeachLessonConditionComponent : Component
{
    /// <summary>
    ///     How close the assassin must be to the person "Being given a lesson", to ensure that the kill is reasonably
    ///     something that could be the assassin's doing. This way the objective isn't resolved by the target getting killed
    ///     by a space tick while on expedition.
    /// </summary>
    [DataField]
    public float MaxDistance = 30f;
}
