using Content.Server.Abilities.Oni;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._EE.Item;

public sealed class OniOnlySystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OniOnlyComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private void OnMeleeAttempt(EntityUid uid, OniOnlyComponent component, ref AttemptMeleeEvent args)
    {
        bool CanUse(EntityUid? uid) => HasComp<OniComponent>(uid) || HasComp<GhostComponent>(uid);

        if (CanUse(args.PlayerUid))
            return;

        // Apply knockdown and force drop before showing popup
        KnockdownAndDropItem(component, args.PlayerUid);

        // Convert entity IDs to display names using the identity system
        var itemName = Identity.Entity(component.Owner, EntityManager);
        var userName = Identity.Entity(args.PlayerUid, EntityManager);
        var selfMessage = Loc.GetString("oni-only-component-attack-fail-self", ("item", itemName));
        var othersMessage = Loc.GetString("oni-only-component-attack-fail-other", ("user", userName), ("item", itemName));

        _popupSystem.PopupPredicted(selfMessage, othersMessage, args.PlayerUid, args.PlayerUid);

        args.Cancelled = true;
    }

    private void KnockdownAndDropItem(OniOnlyComponent component, EntityUid user)
    {
        _stun.TryKnockdown(user, component.KnockdownDuration, true);
        _hands.TryDrop(user);
    }
}
