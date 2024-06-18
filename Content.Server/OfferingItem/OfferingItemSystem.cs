using Content.Shared.Hands.Components;
using Content.Shared.Alert;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.OfferingItem;

namespace Content.Server.OfferingItem;

public sealed class OfferingItemSystem : SharedOfferingItemSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OfferingItemComponent>();
        while (query.MoveNext(out var uid, out var offeringItem))
        {
            if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHand == null)
                continue;

            if (offeringItem.Hand != null &&
                hands.Hands[offeringItem.Hand].HeldEntity == null)
                UnOffer(uid, offeringItem);

            if (!offeringItem.IsInReceiveMode)
            {
                _alertsSystem.ClearAlert(uid, AlertType.Offering);
                continue;
            }

            _alertsSystem.ShowAlert(uid, AlertType.Offering);
        }
    }

    public void Receiving(EntityUid uid, OfferingItemComponent? component = null)
    {
        if (!Resolve(uid, ref component) ||
            !TryComp<OfferingItemComponent>(component.Target, out var offeringItem) ||
            offeringItem.Hand == null ||
            !TryComp<HandsComponent>(uid, out var hands) ||
            !TryComp<HandsComponent>(component.Target, out var handsTarget))
            return;

        var item = handsTarget.Hands[offeringItem.Hand].HeldEntity;
        _hands.TryPickup(component.Target.GetValueOrDefault(), item.GetValueOrDefault(), handsComp:hands);

        UnOffer(uid, component);
    }
}
