using Content.Shared.ActionBlocker;
using Content.Shared.Input;
using Content.Shared.Hands.Components;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Shared.OfferingItem;

public abstract partial class SharedOfferingItemSystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    private void InitializeInteractions()
    {
        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OfferingItem, InputCmdHandler.FromDelegate(SetInOfferMode, handle: false, outsidePrediction: false))
            .Register<SharedOfferingItemSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        CommandBinds.Unregister<SharedOfferingItemSystem>();
    }

    private void SetInOfferMode(ICommonSession? session)
    {
        if (session is not { } playerSession)
            return;

        if ((playerSession.AttachedEntity is not { Valid: true } uid || !Exists(uid)) ||
            !_actionBlocker.CanInteract(uid, null))
            return;

        if (!TryComp<OfferingItemComponent>(uid, out var offeringItem))
            return;

        if (!TryComp<HandsComponent>(uid, out var hands) || hands.ActiveHand == null)
            return;

        var handItem = hands.ActiveHand.HeldEntity;

        if (offeringItem.IsInOfferMode == false)
        {
            if (handItem == null)
                return;

            if (offeringItem.Hand == null || offeringItem.Target == null)
            {
                offeringItem.IsInOfferMode = true;
                offeringItem.Hand = hands.ActiveHand.Name;

                Dirty(uid, offeringItem);
                return;
            }

            if (TryComp<OfferingItemComponent>(offeringItem.Target, out var offeringItemTarget))
            {
                offeringItemTarget.IsInReceiveMode = false;
                offeringItemTarget.Target = null;

                Dirty(offeringItem.Target.Value, offeringItemTarget);
            }
        }

        offeringItem.Hand = null;
        offeringItem.IsInOfferMode = false;
        offeringItem.Target = null;

        Dirty(uid, offeringItem);
    }
}
