using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.WhiteDream.BloodCult.Components;

namespace Content.Shared.WhiteDream.BloodCult.Systems;

public sealed class CultItemSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultItemComponent, BeforeThrowEvent>(OnBeforeThrow);
        SubscribeLocalEvent<CultItemComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<CultItemComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private void OnEquipAttempt(Entity<CultItemComponent> uid, ref BeingEquippedAttemptEvent args)
    {
        if (CanUse(args.EquipTarget))
            return;

        args.Cancel();
        _popupSystem.PopupClient(Loc.GetString("cult-item-component-equip-fail"), uid, args.Equipee);
    }

    private void OnMeleeAttempt(Entity<CultItemComponent> ent, ref AttemptMeleeEvent args)
    {
        if (CanUse(args.PlayerUid))
            return;

        args.Cancelled = true;
        args.Message = Loc.GetString("cult-item-component-attack-fail");
    }

    private void OnBeforeThrow(Entity<CultItemComponent> ent, ref BeforeThrowEvent args)
    {
        if (CanUse(args.PlayerUid))
            return;

        args.Cancelled = true;
        _popupSystem.PopupEntity(Loc.GetString("cult-item-component-throw-fail"), ent, args.PlayerUid);
    }

    private bool CanUse(EntityUid? uid)
    {
        return HasComp<BloodCultistComponent>(uid) || HasComp<GhostComponent>(uid);
    }
}
