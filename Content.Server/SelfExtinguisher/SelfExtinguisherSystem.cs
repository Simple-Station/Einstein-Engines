using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.SelfExtinguisher;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server.SelfExtinguisher;

public sealed partial class SelfExtinguisherSystem : SharedSelfExtinguisherSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SelfExtinguisherComponent, SelfExtinguishEvent>(OnSelfExtinguish);
    }

    private void OnSelfExtinguish(EntityUid uid, SelfExtinguisherComponent component, SelfExtinguishEvent args)
    {
        TryExtinguish(args.Performer, uid, component);
    }

    private void TryExtinguish(EntityUid user, EntityUid uid, SelfExtinguisherComponent? selfExtinguisher = null)
    {
        if (!_container.TryGetContainingContainer((uid, null, null), out var container) ||
            !TryComp<FlammableComponent>(container.Owner, out var flammable) ||
            !Resolve(uid, ref selfExtinguisher))
            return;

        var target = container.Owner;
        var targetIdentity = Identity.Entity(target, EntityManager);
        var locSuffix = user == target ? "self" : "other";

        var curTime = _timing.CurTime;
        if (TryComp<LimitedChargesComponent>(uid, out var charges) &&
            _charges.IsEmpty(uid, charges))
        {
            if (!SetPopupCooldown((uid, selfExtinguisher), curTime))
                return;

            _popup.PopupEntity(Loc.GetString("self-extinguisher-no-charges", ("item", uid)),
                target, user, !flammable.OnFire ? PopupType.Small : PopupType.SmallCaution);
            return;
        }

        if (selfExtinguisher.NextExtinguish > curTime)
        {
            if (!SetPopupCooldown((uid, selfExtinguisher), curTime))
                return;

            _popup.PopupEntity(Loc.GetString($"self-extinguisher-on-cooldown", ("item", uid)),
                target, user, !flammable.OnFire ? PopupType.Small : PopupType.SmallCaution);
            return;
        }

        if (!flammable.OnFire)
        {
            if (!SetPopupCooldown((uid, selfExtinguisher), curTime))
                return;

            _popup.PopupEntity(Loc.GetString($"self-extinguisher-not-on-fire-{locSuffix}", ("item", uid), ("target", targetIdentity)),
                target, user);
            return;
        }

        if (selfExtinguisher.RequiresIgniteFromGasImmune &&
            // Non-self-igniters can use the self-extinguish whenever, but self-igniters must have
            // all ignitable body parts covered up
            (!TryComp<IgniteFromGasComponent>(target, out var ignite) || !ignite.HasImmunity))
        {
            if (!SetPopupCooldown((uid, selfExtinguisher), curTime))
                return;

            _popup.PopupEntity(Loc.GetString($"self-extinguisher-not-immune-to-fire-{locSuffix}", ("item", uid), ("target", targetIdentity)),
                target, user, PopupType.SmallCaution);
            return;
        }

        _flammable.Extinguish(target, flammable);
        _audio.PlayPvs(selfExtinguisher.Sound, uid, selfExtinguisher.Sound.Params.WithVariation(0.125f));
        // TODO add visuals like stam damage for being extinguished

        _popup.PopupPredicted(
            Loc.GetString("self-extinguisher-extinguish-other", ("item", uid), ("target", targetIdentity)),
            target, target, PopupType.Medium
        );
        _popup.PopupEntity(
            Loc.GetString("self-extinguisher-extinguish-self", ("item", uid)),
            target, target, PopupType.Medium
        );

        if (charges != null)
        {
            _charges.UseCharge(uid, charges);
            _actions.RemoveCharges(selfExtinguisher.ActionEntity, 1);

            if (_actions.GetCharges(selfExtinguisher.ActionEntity) == 0)
            {
                _actions.SetEnabled(selfExtinguisher.ActionEntity, false);
                return; // Don't set cooldown when out of charges, they can't use it anymore anyways
            }
        }

        selfExtinguisher.NextExtinguish = curTime + selfExtinguisher.Cooldown;
        _actions.StartUseDelay(selfExtinguisher.ActionEntity);

        Dirty(uid, selfExtinguisher);
    }

    // <summary>
    //   Returns:
    //   - true if a popup is ready to be shown. The popup cooldown is also set.
    //   - false if popups are still on cooldown
    // </summary>
    private bool SetPopupCooldown(Entity<SelfExtinguisherComponent> ent, TimeSpan curTime)
    {
        if (curTime < ent.Comp.NextPopup)
            return false;

        ent.Comp.NextPopup = curTime + ent.Comp.PopupCooldown;
        return true;
    }
}