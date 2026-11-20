/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._CE.ZLevels.Pulling;

/// <summary>
/// Component added to a pulled entity when the puller transitions to another z-level.
/// Handles smooth movement of the pulled entity towards the puller's position until
/// the pulled entity also transitions to the target z-level.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, UnsavedComponent, Access(typeof(CEZLevelPullingSystem))]
public sealed partial class CEZLevelPullingTransitionComponent : Component
{
    /// <summary>
    /// The starting world position of the pulled entity when the transition began.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2 StartPosition;

    /// <summary>
    /// The position of the puller when the transition began (target position to move towards).
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2 TargetPosition;

    /// <summary>
    /// Reference to the puller entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? TargetPuller;

    /// <summary>
    /// The z-level where the puller is now.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TargetZLevel;

    /// <summary>
    /// Time when the transition should be complete.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan? NextTransition;

    /// <summary>
    /// How fast the entity moves during z-level transition (units per second).
    /// </summary>
    [DataField]
    public float TransitionSpeed = 5f;
}
