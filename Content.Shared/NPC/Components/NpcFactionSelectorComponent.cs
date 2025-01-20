using Content.Shared.NPC.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.NPC.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(NpcFactionSelectorSystem))]
public sealed partial class NpcFactionSelectorComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
    public List<string> SelectableFactions = new();
}

