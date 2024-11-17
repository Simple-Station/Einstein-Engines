using Content.Shared.Body.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Content.Shared.Medical.Surgery.Tools;

namespace Content.Shared.Body.Organ;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedBodySystem))]
public sealed partial class OrganComponent : Component, ISurgeryToolComponent
{
    /// <summary>
    /// Relevant body this organ is attached to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    /// <summary>
    /// Shitcodey solution to not being able to know what name corresponds to each organ's slot ID
    /// without referencing the prototype or hardcoding.
    /// </summary>

    [DataField]
    public string SlotId = "";

    [DataField]
    public string ToolName { get; set; } = "An organ";

    /// <summary>
    ///  If true, the organ will not heal an entity when transplanted into them.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool? Used { get; set; }
}
