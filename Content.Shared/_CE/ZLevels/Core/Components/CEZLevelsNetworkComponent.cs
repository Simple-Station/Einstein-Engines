/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CE.ZLevels.Core.Components;

/// <summary>
/// Tracker that tracks all maps added to the zLevel network. Usually, entity in Nullspace,
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(CESharedZLevelsSystem))]
public sealed partial class CEZLevelsNetworkComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<int, EntityUid?> ZLevels = new();

    /// <summary>
    /// Shared components for all zLevels maps
    /// </summary>
    [DataField(serverOnly: true)]
    public ComponentRegistry Components = new();
}
