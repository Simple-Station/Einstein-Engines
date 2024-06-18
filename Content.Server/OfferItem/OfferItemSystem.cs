using Content.Shared.Hands.Components;
using Content.Shared.Alert;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.OfferItem;

namespace Content.Server.OfferItem;

public sealed class OfferItemSystem : SharedOfferItemSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OfferItemComponent>();
        while (query.MoveNext(out var uid, out var offerItem))
        {
            if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHand == null)
                continue;

            if (offerItem.Hand != null &&
                hands.Hands[offerItem.Hand].HeldEntity == null)
                UnOffer(uid, offerItem);

            if (!offerItem.IsInReceiveMode)
            {
                _alertsSystem.ClearAlert(uid, AlertType.Offer);
                continue;
            }

            _alertsSystem.ShowAlert(uid, AlertType.Offer);
        }
    }

    public void Receiving(EntityUid uid, OfferItemComponent? component = null)
    {
        if (!Resolve(uid, ref component) ||
            !TryComp<OfferItemComponent>(component.Target, out var offerItem) ||
            offerItem.Hand == null ||
            !TryComp<HandsComponent>(uid, out var hands) ||
            !TryComp<HandsComponent>(component.Target, out var handsTarget))
            return;

        var item = handsTarget.Hands[offerItem.Hand].HeldEntity;
        _hands.TryPickup(component.Target.GetValueOrDefault(), item.GetValueOrDefault(), handsComp:hands);

        UnOffer(uid, component);
    }
}
