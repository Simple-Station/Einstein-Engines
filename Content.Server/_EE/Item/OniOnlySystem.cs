using Content.Server.Abilities.Oni;
using Content.Shared.ActionBlocker;
using Content.Shared.Hands;


namespace Content.Server._EE.Item;

public sealed class OniOnlySystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OniOnlyComponent, EquippedHandEvent>(OnAttemptPickup);
    }

    private void OnAttemptPickup(EntityUid uid, OniOnlyComponent comp, EquippedHandEvent args)
    {
        if (!HasComp<OniComponent>(args.User))
            args.Handled = true;
        return;

    }
}
