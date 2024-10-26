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

    [DataField]
    public string ToolName { get; set; } = "An organ";
}
