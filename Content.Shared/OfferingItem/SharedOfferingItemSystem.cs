using Content.Shared.Interaction;

namespace Content.Shared.OfferingItem;

public abstract partial class SharedOfferingItemSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<OfferingItemComponent, AfterInteractUsingEvent>(SetInReceiveMode);
        SubscribeLocalEvent<OfferingItemComponent, MoveEvent>(OnMove);

        InitializeInteractions();
    }

    private void SetInReceiveMode(EntityUid uid, OfferingItemComponent component, AfterInteractUsingEvent args)
    {
        if (!TryComp<OfferingItemComponent>(args.User, out var offeringItem))
            return;

        component.IsInReceiveMode = true;
        component.Target = args.User;

        Dirty(uid, component);

        offeringItem.Target = uid;
        offeringItem.IsInOfferMode = false;

        Dirty(args.User, offeringItem);
    }

    private void OnMove(EntityUid uid, OfferingItemComponent component, MoveEvent args)
    {
        if (component.Target == null ||
            args.NewPosition.InRange(EntityManager, _transform,
                Transform(component.Target.GetValueOrDefault()).Coordinates, component.MaxOfferDistance))
            return;

        UnOffer(uid, component);
    }

    protected void UnOffer(EntityUid uid, OfferingItemComponent component)
    {
        if (TryComp<OfferingItemComponent>(component.Target, out var offeringItem) && component.Target != null)
        {
            offeringItem.IsInOfferMode = false;
            offeringItem.IsInReceiveMode = false;
            offeringItem.Hand = null;
            offeringItem.Target = null;

            Dirty(component.Target.Value, offeringItem);
        }

        component.IsInOfferMode = false;
        component.IsInReceiveMode = false;
        component.Hand = null;
        component.Target = null;

        Dirty(uid, component);
    }

    public bool IsInOfferMode(EntityUid? entity, OfferingItemComponent? component = null)
    {
        return entity != null && Resolve(entity.Value, ref component, false) && component.IsInOfferMode;
    }
}
