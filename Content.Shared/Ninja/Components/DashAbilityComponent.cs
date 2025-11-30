using Content.Shared.Actions;
using Content.Shared.Ninja.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Ninja.Components;

/// <summary>
/// Adds an action to dash, teleport to clicked position, when this item is held.
/// Cancel <see cref="CheckDashEvent"/> to prevent using it.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(DashAbilitySystem)), AutoGenerateComponentState]
public sealed partial class DashAbilityComponent : Component
{
    /// <summary>
    /// The action id for dashing.
    /// </summary>
    [DataField]
    public EntProtoId<WorldTargetActionComponent> DashAction = "ActionEnergyKatanaDash";

    [DataField, AutoNetworkedField]
    public EntityUid? DashActionEntity;

    // If true, this item can dash to tiles even if they're not visible/unoccluded to the user.
    [DataField("allowDashToUnseen")]
    public bool AllowDashToUnseen = false;
}

public sealed partial class DashEvent : WorldTargetActionEvent;
