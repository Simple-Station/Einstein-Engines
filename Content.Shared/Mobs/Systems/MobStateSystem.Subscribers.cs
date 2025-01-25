using Content.Shared.Bed.Sleep;
using Content.Shared.Buckle.Components;
using Content.Shared.CCVar;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Pointing;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Strip.Components;
using Content.Shared.Throwing;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Reflection;
using Robust.Shared.Utility;
using System.ComponentModel;

namespace Content.Shared.Mobs.Systems;

public partial class MobStateSystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IReflectionManager _wehavereflectionathome = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    //General purpose event subscriptions. If you can avoid it register these events inside their own systems
    private void SubscribeEvents()
    {
        SubscribeLocalEvent<MobStateComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<MobStateComponent, BeforeGettingStrippedEvent>(OnGettingStripped);
        SubscribeLocalEvent<MobStateComponent, SpeakAttemptEvent>(OnSpeakAttempt);
        SubscribeLocalEvent<MobStateComponent, IsEquippingAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<MobStateComponent, IsUnequippingAttemptEvent>(OnUnequipAttempt);
        SubscribeLocalEvent<MobStateComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<MobStateComponent, TryingToSleepEvent>(OnSleepAttempt);
        SubscribeLocalEvent<MobStateComponent, ChangeDirectionAttemptEvent>(OnDirectionAttempt);
        SubscribeLocalEvent<MobStateComponent, UseAttemptEvent>(CheckActFactory(c => c.CanUse()));
        SubscribeLocalEvent<MobStateComponent, AttackAttemptEvent>(CheckActFactory(c => c.CanAttack()));
        SubscribeLocalEvent<MobStateComponent, ConsciousAttemptEvent>(CheckActFactory(c => c.IsConscious()));
        SubscribeLocalEvent<MobStateComponent, InteractionAttemptEvent>(CheckActFactory(c => !c.IsIncapacitated()));
        SubscribeLocalEvent<MobStateComponent, ThrowAttemptEvent>(CheckActFactory(c => c.CanThrow()));
        SubscribeLocalEvent<MobStateComponent, EmoteAttemptEvent>(CheckActFactory(c => c.CanEmote()));
        SubscribeLocalEvent<MobStateComponent, DropAttemptEvent>(CheckActFactory(c => c.CanPickUp()));
        SubscribeLocalEvent<MobStateComponent, PickupAttemptEvent>(CheckActFactory(c => c.CanPickUp()));
        SubscribeLocalEvent<MobStateComponent, StartPullAttemptEvent>(CheckActFactory(c => c.CanPull()));
        SubscribeLocalEvent<MobStateComponent, StandAttemptEvent>(CheckActFactory(c => !c.IsDowned()));
        SubscribeLocalEvent<MobStateComponent, PointAttemptEvent>(CheckActFactory(c => c.CanPoint()));
        SubscribeLocalEvent<MobStateComponent, CombatModeShouldHandInteractEvent>(OnCombatModeShouldHandInteract);
        SubscribeLocalEvent<MobStateComponent, AttemptPacifiedAttackEvent>(OnAttemptPacifiedAttack);

        SubscribeLocalEvent<MobStateComponent, UnbuckleAttemptEvent>(OnUnbuckleAttempt);
    }

    private void OnComponentInit(EntityUid uid, MobStateComponent comp, ComponentInit args)
    {
        foreach(var entry in comp.InitMobStateParams)
        {
            Enum? e = null;
            DebugTools.Assert(_wehavereflectionathome.TryParseEnumReference($"enum.MobState.{entry.Key}", out e), $"MobState.{entry.Key} does not exist.");

            MobState state = (MobState) e;

            // if this fails, then either the prototype has two parameters specified for one mobstate,
            // or we've already received the params from the server before we had the chance to set them
            // ourselves. (see TraitModifyMobState)
            if (comp.MobStateParams.TryAdd(state, _proto.Index<MobStateParametersPrototype>(entry.Value)))
            {
                //comp.MobStateParams[state].FillDefaults();
            }
        }
    }


    private void OnDirectionAttempt(Entity<MobStateComponent> ent, ref ChangeDirectionAttemptEvent args)
    {
        if (ent.Comp.CanMove())
            return;

        args.Cancel();
    }

    private void OnMoveAttempt(Entity<MobStateComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.CanMove())
            return;

        args.Cancel();
    }


    private void OnUnbuckleAttempt(EntityUid uid, MobStateComponent comp, UnbuckleAttemptEvent args)
    {
        // TODO is this necessary?
        // Shouldn't the interaction have already been blocked by a general interaction check?
        if (args.User.HasValue && IsIncapacitated(args.User.Value))
            args.Cancel();
    }

    private void OnStateExitSubscribers(EntityUid target, MobStateComponent component, MobState state)
    {
        switch (state)
        {
            case MobState.Alive:
                //unused
                break;

            case MobState.SoftCritical:
            case MobState.Critical:
                //if (component.CurrentState is not MobState.Alive)
                //    break;
                //_standing.Stand(target);
                break;
            case MobState.Dead:
                RemComp<CollisionWakeComponent>(target);
                //if (component.CurrentState is MobState.Alive)
                //    _standing.Stand(target);
                //
                //if (!_standing.IsDown(target) && TryComp<PhysicsComponent>(target, out var physics))
                //    _physics.SetCanCollide(target, true, body: physics);
                break;
            case MobState.Invalid:
                //unused
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void OnStateEnteredSubscribers(EntityUid target, MobStateComponent component, MobState state)
    {
        if (component.ShouldDropItems() && HasComp<HandsComponent>(target))
            RaiseLocalEvent(target, new DropHandItemsEvent());

        if (component.IsDowned())
            _standing.Down(target, dropHeldItems: false);
        else
            _standing.Stand(target);

        // All of the state changes here should already be networked, so we do nothing if we are currently applying a
        // server state.
        if (_timing.ApplyingState)
            return;

        _blocker.UpdateCanMove(target); //update movement anytime a state changes
        switch (state)
        {
            case MobState.Alive:
                _appearance.SetData(target, MobStateVisuals.State, MobState.Alive);
                break;
            case MobState.SoftCritical:
            case MobState.Critical:
                _appearance.SetData(target, MobStateVisuals.State, MobState.Critical);
                break;
            case MobState.Dead:
                EnsureComp<CollisionWakeComponent>(target);
                //if (_standing.IsDown(target) && TryComp<PhysicsComponent>(target, out var physics))
                //    _physics.SetCanCollide(target, false, body: physics);

                _appearance.SetData(target, MobStateVisuals.State, MobState.Dead);
                break;
            case MobState.Invalid:
                //unused;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    #region Event Subscribers

    private void OnSleepAttempt(EntityUid target, MobStateComponent component, ref TryingToSleepEvent args)
    {
        if (component.CurrentState is MobState.Alive)
            return;

        args.Cancelled = true;
    }

    private void OnGettingStripped(EntityUid target, MobStateComponent component, BeforeGettingStrippedEvent args)
    {
        args.Multiplier *= component.GetStrippingTimeMultiplier();
    }

    private void OnSpeakAttempt(EntityUid uid, MobStateComponent component, SpeakAttemptEvent args)
    {
        if (HasComp<AllowNextCritSpeechComponent>(uid))
        {
            RemCompDeferred<AllowNextCritSpeechComponent>(uid);
            return;
        }

        if (component.CanTalk())
            return;

        args.Cancel();
    }

    /// <summary>
    /// anti-boilerplate.
    /// Cancels the event if predicate returns false.
    /// </summary>
    private ComponentEventHandler<MobStateComponent, CancellableEntityEventArgs> CheckActFactory(Func<MobStateComponent, bool> predicate)
    {
        return (EntityUid uid, MobStateComponent component, CancellableEntityEventArgs args) =>
        {
            if (!predicate(component))
                args.Cancel();
        };
    }

    //private void CheckAct(EntityUid target, MobStateComponent component, CancellableEntityEventArgs args)
    //{
    //    if (!CanHandInteract(target, component))
    //        args.Cancel();
    //}

    private void OnEquipAttempt(EntityUid uid, MobStateComponent component, IsEquippingAttemptEvent args)
    {
        // is this a self-equip, or are they being stripped?
        if (uid == args.Equipee)
            if (uid == args.EquipTarget && !component.CanEquipSelf() ||
                uid != args.EquipTarget && !component.CanEquipOther())
                args.Cancel();
    }

    private void OnUnequipAttempt(EntityUid uid, MobStateComponent component, IsUnequippingAttemptEvent args)
    {
        // is this a self-equip, or are they being stripped?
        if (uid == args.Unequipee)
            if (uid == args.UnEquipTarget && !component.CanUnequipSelf() ||
                uid != args.UnEquipTarget && !component.CanUnequipOther())
                args.Cancel();
    }

    private void OnCombatModeShouldHandInteract(EntityUid uid, MobStateComponent component, ref CombatModeShouldHandInteractEvent args)
    {
        // Disallow empty-hand-interacting in combat mode
        // for non-dead mobs
        if (!IsDead(uid, component))
            args.Cancelled = true;
    }

    private void OnAttemptPacifiedAttack(Entity<MobStateComponent> ent, ref AttemptPacifiedAttackEvent args)
    {
        args.Cancelled = true;
    }

    #endregion
}
