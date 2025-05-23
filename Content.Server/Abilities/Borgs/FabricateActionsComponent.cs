using Robust.Shared.Prototypes;

namespace Content.Server.Abilities.Borgs;

[RegisterComponent]
public sealed partial class FabricateActionsComponent : Component
{
    /// <summary>
    ///     IDs of fabrication actions that the entity should receive with this component.
    /// </summary>
    [DataField]
    public List<EntProtoId> Actions = new();

    /// <summary>
    ///     Action entities added by this component.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, EntityUid> ActionEntities = new();
}
