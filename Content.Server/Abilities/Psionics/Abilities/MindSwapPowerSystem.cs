using Content.Shared.Actions;
using Content.Server.Psionics;
using Content.Shared.Psionics;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Speech;
using Content.Shared.Stealth.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;
using Content.Shared.Damage;
using Content.Server.Mind;
using Content.Shared.Mobs.Systems;
using Content.Server.Popups;
using Content.Server.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Actions.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.Abilities.Psionics;

public sealed class MindSwapPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _sharedPsionics = default!;
    [Dependency] private readonly PsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindSwapPowerComponent, MindSwapPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<MindSwappedComponent, MindSwapPowerReturnActionEvent>(OnPowerReturned);
        SubscribeLocalEvent<MindSwappedComponent, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<MindSwappedComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<GhostAttemptHandleEvent>(OnGhostAttempt);
        SubscribeLocalEvent<MindSwappedComponent, ComponentShutdown>(OnSwapShutdown);
    }

    private void OnPowerUsed(EntityUid uid, MindSwapPowerComponent component, MindSwapPowerActionEvent args)
    {

        if (!(TryComp<DamageableComponent>(args.Target, out var damageable) && damageable.DamageContainerID == "Biological"))
            return;

        Swap(args.Performer, args.Target);

        _sharedPsionics.LogPowerUsed(args.Performer, "mind swap", 8, 12);
        args.Handled = true;
    }

    private void OnPowerReturned(EntityUid uid, MindSwappedComponent component, MindSwapPowerReturnActionEvent args)
    {
        if (HasComp<PsionicInsulationComponent>(component.OriginalEntity) || HasComp<PsionicInsulationComponent>(uid))
            return;

        if (HasComp<MobStateComponent>(uid) && !_mobStateSystem.IsAlive(uid))
            return;

        // How do we get trapped?
        // 1. Original target doesn't exist
        if (!component.OriginalEntity.IsValid() || Deleted(component.OriginalEntity))
        {
            GetTrapped(uid);
            return;
        }
        // 1. Original target is no longer mindswapped
        if (!TryComp<MindSwappedComponent>(component.OriginalEntity, out var targetMindSwap))
        {
            GetTrapped(uid);
            return;
        }

        // 2. Target has undergone a different mind swap
        if (targetMindSwap.OriginalEntity != uid)
        {
            GetTrapped(uid);
            return;
        }

        // 3. Target is dead
        if (HasComp<MobStateComponent>(component.OriginalEntity) && _mobStateSystem.IsDead(component.OriginalEntity))
        {
            GetTrapped(uid);
            return;
        }

        Swap(uid, component.OriginalEntity, true);
    }

    private void OnDispelled(EntityUid uid, MindSwappedComponent component, DispelledEvent args)
    {
        Swap(uid, component.OriginalEntity, true);
        args.Handled = true;
    }

    private void OnMobStateChanged(EntityUid uid, MindSwappedComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead || args.NewMobState == MobState.Critical)
            Swap(uid, component.OriginalEntity, true);
    }

    private void OnGhostAttempt(GhostAttemptHandleEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<MindSwappedComponent>(args.Mind.CurrentEntity))
            return;

        //No idea where the viaCommand went. It's on the internal OnGhostAttempt, but not this layer. Maybe unnecessary.
        /*if (!args.viaCommand)
            return;*/

        args.Result = false;
        args.Handled = true;
    }

    private void OnSwapShutdown(EntityUid uid, MindSwappedComponent component, ComponentShutdown args)
    {
        if (!_protoMan.TryIndex<PsionicPowerPrototype>("MindSwapReturnPower", out var proto))
            return;
        _actions.RemoveAction(uid, component.MindSwapReturnActionEntity);
        if (TryComp<PsionicComponent>(uid, out var psionic))
            psionic.ActivePowers.Remove(proto);
    }

    public void Swap(EntityUid performer, EntityUid target, bool end = false)
    {
        if (!_protoMan.TryIndex<PsionicPowerPrototype>("MindSwapReturnPower", out var proto)
            || end && (!HasComp<MindSwappedComponent>(performer) || !HasComp<MindSwappedComponent>(target)))
            return;

        // Get the minds first. On transfer, they'll be gone.
        MindComponent? performerMind = null;
        MindComponent? targetMind = null;

        // This is here to prevent missing MindContainerComponent Resolve errors.
        if (!_mindSystem.TryGetMind(performer, out var performerMindId, out performerMind))
        {
            performerMind = null;
        };

        if (!_mindSystem.TryGetMind(target, out var targetMindId, out targetMind))
        {
            targetMind = null;
        };
        //This is a terrible way to 'unattach' minds. I wanted to use UnVisit but in TransferTo's code they say
        //To unnatch the minds, do it like this.
        //Have to unnattach the minds before we reattach them via transfer. Still feels weird, but seems to work well.
        _mindSystem.TransferTo(performerMindId, null);
        _mindSystem.TransferTo(targetMindId, null);
        // Do the transfer.
        if (performerMind != null)
            _mindSystem.TransferTo(performerMindId, target, ghostCheckOverride: true, false, performerMind);

        if (targetMind != null)
            _mindSystem.TransferTo(targetMindId, performer, ghostCheckOverride: true, false, targetMind);

        if (end)
        {
            var performerMindPowerComp = EntityManager.GetComponent<MindSwappedComponent>(performer);
            var targetMindPowerComp = EntityManager.GetComponent<MindSwappedComponent>(target);
            _actions.RemoveAction(performer, performerMindPowerComp.MindSwapReturnActionEntity);
            _actions.RemoveAction(target, targetMindPowerComp.MindSwapReturnActionEntity);

            RemComp<MindSwappedComponent>(performer);
            RemComp<MindSwappedComponent>(target);
            return;
        }

        var perfComp = EnsureComp<MindSwappedComponent>(performer);
        var targetComp = EnsureComp<MindSwappedComponent>(target);

        perfComp.OriginalEntity = target;
        targetComp.OriginalEntity = performer;

        _psionics.InitializePsionicPower(performer, proto);
        _psionics.InitializePsionicPower(target, proto);
    }

    //It shouldn't actually be possible anymore to get trapped under most circumstances, but for niche edge cases, I am leaving this here
    public void GetTrapped(EntityUid uid)
    {
        _popupSystem.PopupEntity(Loc.GetString("mindswap-trapped"), uid, uid, Shared.Popups.PopupType.LargeCaution);
        var perfComp = EnsureComp<MindSwappedComponent>(uid);
        _actions.RemoveAction(uid, perfComp.MindSwapReturnActionEntity, null);

        if (HasComp<TelegnosticProjectionComponent>(uid))
        {
            RemComp<PsionicallyInvisibleComponent>(uid);
            RemComp<StealthComponent>(uid);
            EnsureComp<SpeechComponent>(uid);
            EnsureComp<DispellableComponent>(uid);
            _metaDataSystem.SetEntityName(uid, Loc.GetString("telegnostic-trapped-entity-name"));
            _metaDataSystem.SetEntityDescription(uid, Loc.GetString("telegnostic-trapped-entity-desc"));
        }
    }
}
