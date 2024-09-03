using Content.Shared.Construction.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._White.ShortConstruction;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShortConstructionComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<ConstructionPrototype>> Prototypes = new();
}

[NetSerializable, Serializable]
public enum ShortConstructionUiKey : byte
{
    Key,
}
