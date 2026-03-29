/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._CE.ZLevels.Flight.Components;
using Content.Shared.Actions;
using Content.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Gravity;
using Content.Shared.Mobs;
using Content.Shared.Stunnable;
using JetBrains.Annotations;
using Robust.Shared.Serialization;

namespace Content.Shared._CE.ZLevels.Flight;

public abstract partial class CESharedZFlightSystem : EntitySystem
{
    [Dependency] private readonly CESharedZLevelsSystem _zLevel = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;

    protected EntityQuery<CEZPhysicsComponent> ZPhyzQuery;

    public override void Initialize()
    {
        base.Initialize();
        InitializeControllable();

        ZPhyzQuery = GetEntityQuery<CEZPhysicsComponent>();

        SubscribeLocalEvent<CEZPhysicsComponent, CEFlightStartedEvent>(OnStartFlight);
        SubscribeLocalEvent<CEZPhysicsComponent, CEFlightStoppedEvent>(OnStopFlight);
        SubscribeLocalEvent<CEZFlyerComponent, CEGetZVelocityEvent>(OnGetZVelocity);
        SubscribeLocalEvent<CEZFlyerComponent, CECheckGravityEvent>(OnGetGravity);
        SubscribeLocalEvent<CEZFlyerComponent, IsWeightlessEvent>(CheckWeightless);

        SubscribeLocalEvent<CEZFlyerComponent, StunnedEvent>(OnStunned);
        SubscribeLocalEvent<CEZFlyerComponent, KnockedDownEvent>(OnKnockDowned);
        SubscribeLocalEvent<CEZFlyerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<CEZFlyerComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void CheckWeightless(Entity<CEZFlyerComponent> ent, ref IsWeightlessEvent args)
    {
        if (!ent.Comp.Active || args.Handled)
            return;

        args.IsWeightless = true;
        args.Handled = true;
    }

    private void OnDamageChanged(Entity<CEZFlyerComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        if (!args.InterruptsDoAfters)
            return;

        DeactivateFlight((ent, ent));
    }

    private void OnMobStateChanged(Entity<CEZFlyerComponent> ent, ref MobStateChangedEvent args)
    {
        DeactivateFlight((ent, ent));
    }

    private void OnKnockDowned(Entity<CEZFlyerComponent> ent, ref KnockedDownEvent args)
    {
        DeactivateFlight((ent, ent));
    }

    private void OnStunned(Entity<CEZFlyerComponent> ent, ref StunnedEvent args)
    {
        DeactivateFlight((ent, ent));
    }

    private void OnStartFlight(Entity<CEZPhysicsComponent> ent, ref CEFlightStartedEvent args)
    {
        SetTargetHeight(ent.Owner, ent.Comp.CurrentZLevel);
        StartFlightVisuals(ent.Owner);
    }

    private void OnStopFlight(Entity<CEZPhysicsComponent> ent, ref CEFlightStoppedEvent args)
    {
        StopFlightVisuals(ent.Owner);
    }

    private void OnGetZVelocity(Entity<CEZFlyerComponent> ent, ref CEGetZVelocityEvent args)
    {
        if (!ent.Comp.Active)
            return;

        var zPhys = args.Target.Comp;
        var currentPos = zPhys.CurrentZLevel + zPhys.LocalPosition;
        var targetPos = ent.Comp.TargetMapHeight + 0.2f;
        var currentVelocity = zPhys.Velocity;

        var distanceToTarget = targetPos - currentPos;

        var targetVelocity = Math.Clamp(distanceToTarget * ent.Comp.FlightSpeed, -ent.Comp.FlightSpeed, ent.Comp.FlightSpeed);
        var velocityDelta = targetVelocity - currentVelocity;

        var upperBound = ent.Comp.TargetMapHeight + 0.9f;
        var lowerBound = ent.Comp.TargetMapHeight + 0.1f;

        var newVelocity = currentVelocity + velocityDelta;
        var nextPos = currentPos + newVelocity;

        if (nextPos > upperBound)
        {
            var maxAllowedVelocity = upperBound - currentPos;
            velocityDelta = maxAllowedVelocity - currentVelocity;
        }
        else if (nextPos < lowerBound)
        {
            var maxAllowedVelocity = lowerBound - currentPos;
            velocityDelta = maxAllowedVelocity - currentVelocity;
        }

        args.VelocityDelta = velocityDelta;
    }

    private void OnGetGravity(Entity<CEZFlyerComponent> ent, ref CECheckGravityEvent args)
    {
        if (ent.Comp.Active)
            args.Gravity *= 0;
    }

    [PublicAPI]
    public bool TryActivateFlight(Entity<CEZFlyerComponent?> ent, CEZPhysicsComponent? zPhys = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (!Resolve(ent, ref zPhys, false))
            return false;

        if (ent.Comp.Active)
            return false;

        var ev = new CEStartFlightAttemptEvent();
        RaiseLocalEvent(ent, ev);

        if (ev.Cancelled)
            return false;

        ent.Comp.Active = true;
        DirtyField(ent, ent.Comp, nameof(CEZFlyerComponent.Active));

        _zLevel.UpdateGravityState((ent, zPhys));
        _gravity.RefreshWeightless(ent.Owner);

        RaiseLocalEvent(ent, new CEFlightStartedEvent());
        return true;
    }

    [PublicAPI]
    public void DeactivateFlight(Entity<CEZFlyerComponent?> ent, CEZPhysicsComponent? zPhys = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!Resolve(ent, ref zPhys, false))
            return;

        if (!ent.Comp.Active)
            return;

        ent.Comp.Active = false;
        DirtyField(ent, ent.Comp, nameof(CEZFlyerComponent.Active));

        _zLevel.UpdateGravityState((ent, zPhys));
        _gravity.RefreshWeightless(ent.Owner);

        RaiseLocalEvent(ent, new CEFlightStoppedEvent());
    }

    [PublicAPI]
    public void SetTargetHeight(Entity<CEZFlyerComponent?> ent, int targetHeight)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.TargetMapHeight = targetHeight;
        DirtyField(ent, ent.Comp, nameof(CEZFlyerComponent.TargetMapHeight));
    }

    private void StartFlightVisuals(Entity<CEZFlyerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        _appearance.SetData(ent, CEFlightVisuals.Active, true);
        _ambient.SetAmbience(ent, true);
    }

    private void StopFlightVisuals(Entity<CEZFlyerComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        _appearance.SetData(ent, CEFlightVisuals.Active, false);
        _ambient.SetAmbience(ent, false);
    }
}

/// <summary>
/// Called on an entity when it attempts to start flight mode. Subscribe and cancel this event if you want to cancel your flight for any reason.
/// </summary>
public sealed class CEStartFlightAttemptEvent : CancellableEntityEventArgs;

/// <summary>
/// Called on an entity when it enters flight mode
/// </summary>
public sealed class CEFlightStartedEvent : EntityEventArgs;

/// <summary>
/// Called on an entity when it exits flight mode
/// </summary>
public sealed class CEFlightStoppedEvent : EntityEventArgs;


/// <summary>
/// Instant Action, raising the target flight level by 1
/// </summary>
public sealed partial class CEZFlightActionUp : InstantActionEvent
{
}

/// <summary>
/// Instant Action, lowering the target flight level by 1
/// </summary>
public sealed partial class CEZFlightActionDown : InstantActionEvent
{
}


[Serializable, NetSerializable]
public enum CEFlightVisuals
{
    Active,
}

/// <summary>
/// DoAfter event for starting flight with a delay
/// </summary>
[Serializable, NetSerializable]
public sealed partial class CEStartFlightDoAfterEvent : SimpleDoAfterEvent
{
}
