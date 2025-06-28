using Content.Shared.Hands.Components;
using Content.Shared.Alert;
using Content.Shared.OfferItem;

namespace Content.Server.OfferItem;

public sealed class OfferItemSystem : SharedOfferItemSystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

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
            {
                if (offerItem.Target != null)
                {
                    UnReceive(offerItem.Target.Value, offerItem: offerItem);
                    offerItem.IsInOfferMode = false;
                    Dirty(uid, offerItem);
                }
                else
                    UnOffer(uid, offerItem);
            }

            if (!offerItem.IsInReceiveMode)
            {
                _alertsSystem.ClearAlert(uid, offerItem.OfferAlert);
                continue;
            }

            _alertsSystem.ShowAlert(uid, offerItem.OfferAlert);
        }
    }
}
