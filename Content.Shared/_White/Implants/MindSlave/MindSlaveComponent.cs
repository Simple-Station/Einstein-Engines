using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._White.Implants.MindSlave;

[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class MindSlaveComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> SlaveStatusIcon = "SlaveMindSlaveFaction";

    [DataField]
    public ProtoId<FactionIconPrototype> MasterStatusIcon = "MasterMindSlaveFaction";

    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public List<NetEntity> Slaves = [];

    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public NetEntity? Master;
}
