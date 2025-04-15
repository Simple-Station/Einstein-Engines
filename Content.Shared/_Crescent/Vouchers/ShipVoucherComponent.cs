using Content.Shared.Shipyard.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Crescent.Vouchers;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShipVoucherComponent : Component
{
    [DataField("ship", customTypeSerializer: typeof(PrototypeIdSerializer<VesselPrototype>))]
    public string Ship;

    [DataField("requiresShipInConsole")]
    public bool RequiresShipInConsole;
}
