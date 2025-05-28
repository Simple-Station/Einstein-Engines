using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// Dynamic component, that is assigned to active fishing spots that are currently waiting for da fish.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveFishingSpotComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? AttachedFishingLure;

    [DataField, AutoNetworkedField]
    public TimeSpan? FishingStartTime;

    /// <summary>
    /// If true, someone is pulling fish out of this spot.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public float FishDifficulty;

    /// <summary>
    /// Fish that we're currently trying to catch
    /// </summary>
    [DataField]
    public EntProtoId? Fish; // not networked because useless for client
}
