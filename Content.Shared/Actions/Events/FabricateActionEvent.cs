using Robust.Shared.Prototypes;

namespace Content.Shared.Actions.Events;

public sealed class FabricateActionEvent : InstantActionEvent
{
    [DataField(required: true)]
    public ProtoId<EntityPrototype> Fabrication;
}
