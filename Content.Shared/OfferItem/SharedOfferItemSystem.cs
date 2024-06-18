using Content.Shared.Interaction;

namespace Content.Shared.OfferItem;

public abstract partial class SharedOfferItemSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<OfferItemComponent, AfterInteractUsingEvent>(SetInReceiveMode);
        SubscribeLocalEvent<OfferItemComponent, MoveEvent>(OnMove);

        InitializeInteractions();
    }

    private void SetInReceiveMode(EntityUid uid, OfferItemComponent component, AfterInteractUsingEvent args)
    {
        if (!TryComp<OfferItemComponent>(args.User, out var offerItem))
            return;

        component.IsInReceiveMode = true;
        component.Target = args.User;

        Dirty(uid, component);

        offerItem.Target = uid;
        offerItem.IsInOfferMode = false;

        Dirty(args.User, offerItem);
    }

    private void OnMove(EntityUid uid, OfferItemComponent component, MoveEvent args)
    {
        if (component.Target == null ||
            args.NewPosition.InRange(EntityManager, _transform,
                Transform(component.Target.GetValueOrDefault()).Coordinates, component.MaxOfferDistance))
            return;

        UnOffer(uid, component);
    }

    protected void UnOffer(EntityUid uid, OfferItemComponent component)
    {
        if (TryComp<OfferItemComponent>(component.Target, out var offerItem) && component.Target != null)
        {
            offerItem.IsInOfferMode = false;
            offerItem.IsInReceiveMode = false;
            offerItem.Hand = null;
            offerItem.Target = null;

            Dirty(component.Target.Value, offerItem);
        }

        component.IsInOfferMode = false;
        component.IsInReceiveMode = false;
        component.Hand = null;
        component.Target = null;

        Dirty(uid, component);
    }

    public bool IsInOfferMode(EntityUid? entity, OfferItemComponent? component = null)
    {
        return entity != null && Resolve(entity.Value, ref component, false) && component.IsInOfferMode;
    }
}
