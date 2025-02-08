using Content.Shared.Bed.Sleep;
using Content.Shared.Buckle.Components;
using Content.Shared.CCVar;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Emoting;
using Content.Shared.Hands;
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

namespace Content.Shared.Mobs.Systems;

public partial class MobStateSystem
{
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;

    //General purpose event subscriptions. If you can avoid it register these events inside their own systems
    private void SubscribeEvents()
    {
        SubscribeLocalEvent<MobStateComponent, BeforeGettingStrippedEvent>(OnGettingStripped);
        SubscribeLocalEvent<MobStateComponent, ChangeDirectionAttemptEvent>(OnDirectionAttempt);
        SubscribeLocalEvent<MobStateComponent, UseAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, AttackAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, ConsciousAttemptEvent>(CheckConcious);
        SubscribeLocalEvent<MobStateComponent, ThrowAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, SpeakAttemptEvent>(OnSpeakAttempt);
        SubscribeLocalEvent<MobStateComponent, IsEquippingAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<MobStateComponent, EmoteAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, IsUnequippingAttemptEvent>(OnUnequipAttempt);
        SubscribeLocalEvent<MobStateComponent, DropAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, PickupAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, StartPullAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, UpdateCanMoveEvent>(OnMoveAttempt);
        SubscribeLocalEvent<MobStateComponent, StandAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, PointAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, TryingToSleepEvent>(OnSleepAttempt);
        SubscribeLocalEvent<MobStateComponent, CombatModeShouldHandInteractEvent>(OnCombatModeShouldHandInteract);
        SubscribeLocalEvent<MobStateComponent, AttemptPacifiedAttackEvent>(OnAttemptPacifiedAttack);

        SubscribeLocalEvent<MobStateComponent, UnbuckleAttemptEvent>(OnUnbuckleAttempt);
    }

    private void OnDirectionAttempt(Entity<MobStateComponent> ent, ref ChangeDirectionAttemptEvent args)
    {
        if (ent.Comp.CurrentState is MobState.Alive
            || ent.Comp.CurrentState is MobState.Critical
            && ent.Comp.AllowMovementWhileCrit
            && _configurationManager.GetCVar(CCVars.AllowMovementWhileCrit)
            || ent.Comp.CurrentState is MobState.SoftCritical
            && ent.Comp.AllowMovementWhileSoftCrit
            || ent.Comp.CurrentState is MobState.Dead
            && ent.Comp.AllowMovementWhileDead)
            return;

        args.Cancel();
    }

    private void OnMoveAttempt(Entity<MobStateComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.CurrentState is MobState.Alive
            || ent.Comp.CurrentState is MobState.Critical
            && ent.Comp.AllowMovementWhileCrit
            && _configurationManager.GetCVar(CCVars.AllowMovementWhileCrit)
            || ent.Comp.CurrentState is MobState.SoftCritical
            && ent.Comp.AllowMovementWhileSoftCrit
            || ent.Comp.CurrentState is MobState.Dead
            && ent.Comp.AllowMovementWhileDead)
            return;

        args.Cancel();
    }


    private void OnUnbuckleAttempt(Entity<MobStateComponent> ent, ref UnbuckleAttemptEvent args)
    {
        // TODO is this necessary?
        // Shouldn't the interaction have already been blocked by a general interaction check?
        if (args.User == ent.Owner && IsIncapacitated(ent))
            args.Cancelled = true;
    }

    private void CheckConcious(Entity<MobStateComponent> ent, ref ConsciousAttemptEvent args)
    {
        switch (ent.Comp.CurrentState)
        {
            case MobState.Dead:
            case MobState.Critical:
                args.Cancelled = true;
                break;
        }
    }

    private void OnStateExitSubscribers(EntityUid target, MobStateComponent component, MobState state)
    {
        switch (state)
        {
            case MobState.Alive:
                //unused
                break;
            case MobState.Critical:
                if (component.CurrentState is not MobState.Alive)
                    break;
                _standing.Stand(target);
                break;
            case MobState.SoftCritical:
                if (component.CurrentState is not MobState.Alive)
                    break;
                _standing.Stand(target);
                break;
            case MobState.Dead:
                RemComp<CollisionWakeComponent>(target);
                if (component.CurrentState is MobState.Alive)
                    _standing.Stand(target);

                if (!_standing.IsDown(target) && TryComp<PhysicsComponent>(target, out var physics))
                    _physics.SetCanCollide(target, true, body: physics);

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
        // All of the state changes here should already be networked, so we do nothing if we are currently applying a
        // server state.
        if (_timing.ApplyingState)
            return;

        _blocker.UpdateCanMove(target); //update movement anytime a state changes
        switch (state)
        {
            case MobState.Alive:
                _standing.Stand(target);
                _appearance.SetData(target, MobStateVisuals.State, MobState.Alive);
                break;
            case MobState.Critical:
                if (component.DownWhenCrit)
                    _standing.Down(target);
                _appearance.SetData(target, MobStateVisuals.State, MobState.Critical);
                break;
            case MobState.SoftCritical:
                if (component.DownWhenSoftCrit)
                    _standing.Down(target);
                _appearance.SetData(target, MobStateVisuals.State, MobState.Critical);
                break;
            case MobState.Dead:
                EnsureComp<CollisionWakeComponent>(target);
                if (component.DownWhenDead)
                    _standing.Down(target);

                if (_standing.IsDown(target) && TryComp<PhysicsComponent>(target, out var physics))
                    _physics.SetCanCollide(target, false, body: physics);

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
        // Incapacitated or dead targets get stripped two or three times as fast. Makes stripping corpses less tedious.
        if (IsDead(target, component))
            args.Multiplier /= 3;
        else if (IsCritical(target, component))
            args.Multiplier /= 2;
    }

    private void OnSpeakAttempt(EntityUid uid, MobStateComponent component, SpeakAttemptEvent args)
    {
        if (HasComp<AllowNextCritSpeechComponent>(uid))
        {
            RemCompDeferred<AllowNextCritSpeechComponent>(uid);
            return;
        }

        if (component.CurrentState is MobState.Alive
            || component.CurrentState is MobState.Critical
            && component.AllowTalkingWhileCrit
            && _configurationManager.GetCVar(CCVars.AllowTalkingWhileCrit)
            || component.CurrentState is MobState.SoftCritical
            && component.AllowTalkingWhileSoftCrit
            || component.CurrentState is MobState.Dead
            && component.AllowTalkingWhileDead)
            return;

        args.Cancel();
    }

    private void CheckAct(EntityUid target, MobStateComponent component, CancellableEntityEventArgs args)
    {
        switch (component.CurrentState)
        {
            case MobState.Dead:
            case MobState.SoftCritical:
            case MobState.Critical:
                args.Cancel();
                break;
        }
    }

    private void OnEquipAttempt(EntityUid target, MobStateComponent component, IsEquippingAttemptEvent args)
    {
        // is this a self-equip, or are they being stripped?
        if (args.Equipee == target)
            CheckAct(target, component, args);
    }

    private void OnUnequipAttempt(EntityUid target, MobStateComponent component, IsUnequippingAttemptEvent args)
    {
        // is this a self-equip, or are they being stripped?
        if (args.Unequipee == target)
            CheckAct(target, component, args);
    }

    private void OnCombatModeShouldHandInteract(EntityUid uid, MobStateComponent component, ref CombatModeShouldHandInteractEvent args)
    {
        if (component.CurrentState is MobState.Alive
            || component.CurrentState is MobState.Critical
            && component.AllowHandInteractWhileCrit
            || component.CurrentState is MobState.SoftCritical
            && component.AllowHandInteractWhileSoftCrit
            || component.CurrentState is MobState.Dead
            && component.AllowHandInteractWhileDead)
            return;

        args.Cancelled = true;
    }

    private void OnAttemptPacifiedAttack(Entity<MobStateComponent> ent, ref AttemptPacifiedAttackEvent args)
    {
        args.Cancelled = true;
    }

    #endregion
}
