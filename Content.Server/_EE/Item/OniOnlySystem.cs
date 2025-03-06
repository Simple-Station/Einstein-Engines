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

        KnockdownAndDropItem(component, args.PlayerUid, "oni-only-component-attack-fail-self");

        args.Cancelled = true;
    }

    private void KnockdownAndDropItem(OniOnlyComponent component, EntityUid user, string message, bool serverOnly = false)
    {
        _stun.TryKnockdown(user, component.KnockdownDuration, true);
        _hands.TryDrop(user);

        if (serverOnly)
            _popupSystem.PopupEntity(message, component.Owner, user);
        else
            _popupSystem.PopupPredicted(message, component.Owner, user);
    }
}
