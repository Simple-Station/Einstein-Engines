using Content.Shared.Examine;
using Content.Shared.Shipyard.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Crescent.Vouchers;

public abstract partial class SharedVoucherSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShipVoucherComponent, ExaminedEvent>(OnShipVoucherExamined);
    }

    private void OnShipVoucherExamined(EntityUid uid, ShipVoucherComponent component, ExaminedEvent args)
    {
        if (!string.IsNullOrEmpty(component.Ship) && _prototypeManager.TryIndex<VesselPrototype>(component.Ship, out var prototype))
        {
            args.PushMarkup(Loc.GetString("ship-voucher-examine", ("ship", prototype.Name)));
        }
    }
}
