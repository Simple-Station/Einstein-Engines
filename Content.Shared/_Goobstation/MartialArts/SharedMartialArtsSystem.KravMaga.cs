using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared.Contests;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
//using Content.Shared.Contests;
using Robust.Shared.Physics.Dynamics;

namespace Content.Shared._Goobstation.MartialArts;

/// <summary>
/// This handles...
/// </summary>
public abstract partial class SharedMartialArtsSystem
{
    private void InitializeKravMaga()
    {
        SubscribeLocalEvent<KravMagaComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<KravMagaComponent, KravMagaActionEvent>(OnKravMagaAction);
        SubscribeLocalEvent<KravMagaComponent, MeleeHitEvent>(OnMeleeHitEvent);
        SubscribeLocalEvent<KravMagaComponent, ComponentShutdown>(OnKravMagaShutdown);
        SubscribeLocalEvent<KravMagaComponent, ComboAttackPerformedEvent>(OnKravMagaAttackPerformed);
    }

    private void OnMeleeHitEvent(Entity<KravMagaComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count <= 0)
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hitEntity))
                continue;
            if (!TryComp<RequireProjectileTargetComponent>(hitEntity, out var isDowned))
                continue;

            DoKravMaga(ent, hitEntity, isDowned);
        }
    }
    private void OnKravMagaAttackPerformed(Entity<KravMagaComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (!TryComp<KravMagaComponent>(ent, out var knowledgeComponent))
            return;

        switch (args.Type)
        {
            // todo: add chances
            /*  case ComboAttackType.Disarm:
                var target = args.Target;
                if (_hands.TryGetActiveItem(target, out var activeItem)
                && _hands.TryGetEmptyHand(target, out var emptyHand)
                && _hands.TryDrop(target, activeItem.Value)
                && _hands.TryPickupAnyHand(ent, activeItem.Value)
                && _hands.TryGetEmptyHand(ent, out var userEmptyHand))
                    _hands.SetActiveHand(ent, userEmptyHand);
                break; */
            case ComboAttackType.Harm:
                DoDamage(ent, args.Target, "Blunt", ent.Comp.BaseDamage, out _);
                if (!TryComp<RequireProjectileTargetComponent>(args.Target, out var standing)
    || !standing.Active)
                    return;
                DoDamage(ent, args.Target, "Blunt", ent.Comp.DownedDamageModifier, out _);
                break;
        }
    }

    private void DoKravMaga(Entity<KravMagaComponent> ent, EntityUid hitEntity, RequireProjectileTargetComponent requireProjectileTargetComponent)
    {
        if (ent.Comp.SelectedMoveComp == null)
            return;
        var moveComp = ent.Comp.SelectedMoveComp;

        switch (ent.Comp.SelectedMove)
        {
            case KravMagaMoves.LegSweep:
                if(_netManager.IsClient)
                    return;
                _stun.TryParalyze(hitEntity, TimeSpan.FromSeconds(4), true);
                break;
            case KravMagaMoves.NeckChop:
                var comp = EnsureComp<KravMagaSilencedComponent>(hitEntity);
                comp.SilencedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.EffectTime);
                break;
            case KravMagaMoves.LungPunch:
                _stamina.TakeStaminaDamage(hitEntity, moveComp.StaminaDamage);
                var blockedBreathingComponent = EnsureComp<KravMagaBlockedBreathingComponent>(hitEntity);
                blockedBreathingComponent.BlockedTime = _timing.CurTime + TimeSpan.FromSeconds(moveComp.EffectTime);
                break;
            case null:
                var damage = ent.Comp.BaseDamage;
                if (requireProjectileTargetComponent.Active)
                    damage *= ent.Comp.DownedDamageModifier;

                DoDamage(ent.Owner, hitEntity, "Blunt", damage, out _);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        ent.Comp.SelectedMove = null;
        ent.Comp.SelectedMoveComp = null;
    }

    private void OnKravMagaAction(Entity<KravMagaComponent> ent, ref KravMagaActionEvent args)
    {
        var actionEnt = args.Action.Owner;
        if (!TryComp<KravMagaActionComponent>(actionEnt, out var kravActionComp))
            return;

        _popupSystem.PopupClient(Loc.GetString("krav-maga-ready", ("action", kravActionComp.Name)), ent, ent);
        ent.Comp.SelectedMove = kravActionComp.Configuration;
        ent.Comp.SelectedMoveComp = kravActionComp;
    }

    private void OnMapInit(Entity<KravMagaComponent> ent, ref MapInitEvent args)
    {
        if (HasComp<MartialArtsKnowledgeComponent>(ent))
            return;
        foreach (var actionId in ent.Comp.BaseKravMagaMoves)
        {
            var actions = _actions.AddAction(ent, actionId);
            if (actions != null)
                ent.Comp.KravMagaMoveEntities.Add(actions.Value);
        }
    }

    private void OnKravMagaShutdown(Entity<KravMagaComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<KravMagaComponent>(ent, out var kravMaga))
            return;

        foreach (var action in ent.Comp.KravMagaMoveEntities)
        {
            _actions.RemoveAction(action);
        }
    }
}
