using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Flight;
using Content.Shared.Flight.Events;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._WoF.Flight;

/// <summary>
///     Altitude bands for dragon flight, layered over the existing <see cref="FlightComponent"/> toggle.
///     <list type="bullet">
///         <item>Ground: normal walking.</item>
///         <item>Low: flying. Passes over tables, crates and half walls. Can still interact with things.</item>
///         <item>High: faster, but can't pick things up or use things on the ground.</item>
///     </list>
///     Walls and doors block every altitude, so flight never bypasses security.
///     State changes run on the server and are networked; the client only draws the height.
/// </summary>
public sealed class DragonFlightSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedFlightSystem _flight = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DragonFlightComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DragonFlightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<DragonFlightComponent, FlightToggledEvent>(OnFlightToggled);
        SubscribeLocalEvent<DragonFlightComponent, DragonClimbActionEvent>(OnClimb);
        SubscribeLocalEvent<DragonFlightComponent, DragonDescendActionEvent>(OnDescend);
        SubscribeLocalEvent<DragonFlightComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
        SubscribeLocalEvent<DragonFlightComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<DragonFlightComponent, InteractionAttemptEvent>(OnInteractionAttempt);
        SubscribeLocalEvent<DragonFlightComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    /// <summary>
    ///     Going critical or dying in the air drops you. (The base flight system only lands you on other
    ///     state changes, so a downed dragon would otherwise keep floating.)
    /// </summary>
    private void OnMobStateChanged(EntityUid uid, DragonFlightComponent comp, MobStateChangedEvent args)
    {
        if (!_net.IsServer || args.NewMobState is not (MobState.Critical or MobState.Dead))
            return;

        if (TryComp<FlightComponent>(uid, out var flight) && flight.On)
            _flight.ToggleActive(uid, false, flight);
    }

    #region Lifecycle

    private void OnMapInit(Entity<DragonFlightComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ClimbActionEntity, ent.Comp.ClimbAction);
        _actions.AddAction(ent, ref ent.Comp.DescendActionEntity, ent.Comp.DescendAction);
        Dirty(ent);
    }

    private void OnShutdown(Entity<DragonFlightComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ClimbActionEntity);
        _actions.RemoveAction(ent.Comp.DescendActionEntity);

        // The wing was removed or disabled while airborne: restore collision and crash.
        if (_net.IsServer && ent.Comp.Altitude != FlightAltitude.Ground && !TerminatingOrDeleted(ent))
        {
            ApplyFallDamage(ent, ent.Comp.Altitude);
            RestoreCollision(ent);
        }
    }

    #endregion

    #region Altitude changes

    private void OnFlightToggled(Entity<DragonFlightComponent> ent, ref FlightToggledEvent args)
    {
        if (args.Active)
        {
            SetAltitude(ent, FlightAltitude.Low);
            return;
        }

        // Flight ended without a controlled landing (Descend sets Ground first), so this is a fall.
        var from = ent.Comp.Altitude;
        SetAltitude(ent, FlightAltitude.Ground);
        if (from != FlightAltitude.Ground)
            ApplyFallDamage(ent, from);
    }

    private void OnClimb(Entity<DragonFlightComponent> ent, ref DragonClimbActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!_net.IsServer)
            return;

        switch (ent.Comp.Altitude)
        {
            case FlightAltitude.Ground:
                _popup.PopupEntity(Loc.GetString("wof-flight-takeoff-first"), ent, ent);
                break;
            case FlightAltitude.Low:
                SetAltitude(ent, FlightAltitude.High);
                _popup.PopupEntity(Loc.GetString("wof-flight-climb"), ent, ent);
                break;
            case FlightAltitude.High:
                _popup.PopupEntity(Loc.GetString("wof-flight-already-high"), ent, ent);
                break;
        }
    }

    private void OnDescend(Entity<DragonFlightComponent> ent, ref DragonDescendActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!_net.IsServer)
            return;

        switch (ent.Comp.Altitude)
        {
            case FlightAltitude.High:
                SetAltitude(ent, FlightAltitude.Low);
                _popup.PopupEntity(Loc.GetString("wof-flight-descend"), ent, ent);
                break;
            case FlightAltitude.Low:
                // Controlled landing: set Ground before turning flight off so it doesn't count as a fall.
                if (TryComp<FlightComponent>(ent, out var flight) && flight.On)
                {
                    SetAltitude(ent, FlightAltitude.Ground);
                    _flight.ToggleActive(ent, false, flight);
                    _popup.PopupEntity(Loc.GetString("wof-flight-land"), ent, ent);
                }
                break;
        }
    }

    /// <summary>
    ///     Moves a dragon to a new altitude, updating collision, speed and networked state.
    ///     Server only; clients receive the result through component state.
    /// </summary>
    public void SetAltitude(Entity<DragonFlightComponent> ent, FlightAltitude altitude)
    {
        if (!_net.IsServer)
            return;

        var old = ent.Comp.Altitude;
        if (old == altitude)
            return;

        if (old == FlightAltitude.Ground)
            RemoveCollision(ent);
        else if (altitude == FlightAltitude.Ground)
            RestoreCollision(ent);

        ent.Comp.Altitude = altitude;
        Dirty(ent);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void RemoveCollision(Entity<DragonFlightComponent> ent)
    {
        if (!TryComp<FixturesComponent>(ent, out var fixtures))
            return;

        ent.Comp.RemovedMaskBits.Clear();
        foreach (var (id, fixture) in fixtures.Fixtures)
        {
            var removed = fixture.CollisionMask & ent.Comp.AirborneIgnoredMask;
            if (removed == 0)
                continue;

            ent.Comp.RemovedMaskBits[id] = removed;
            _physics.SetCollisionMask(ent, id, fixture, fixture.CollisionMask & ~removed, fixtures);
        }
    }

    private void RestoreCollision(Entity<DragonFlightComponent> ent)
    {
        if (!TryComp<FixturesComponent>(ent, out var fixtures))
        {
            ent.Comp.RemovedMaskBits.Clear();
            return;
        }

        foreach (var (id, bits) in ent.Comp.RemovedMaskBits)
        {
            if (!fixtures.Fixtures.TryGetValue(id, out var fixture))
                continue;

            _physics.SetCollisionMask(ent, id, fixture, fixture.CollisionMask | bits, fixtures);
        }

        ent.Comp.RemovedMaskBits.Clear();
    }

    private void ApplyFallDamage(Entity<DragonFlightComponent> ent, FlightAltitude from)
    {
        var damage = from == FlightAltitude.High ? ent.Comp.HighFallDamage : ent.Comp.LowFallDamage;
        if (damage == null)
            return;

        _damageable.TryChangeDamage(ent.Owner, damage);
        if (from == FlightAltitude.High)
            _popup.PopupEntity(Loc.GetString("wof-flight-fall"), ent, ent, PopupType.MediumCaution);
    }

    #endregion

    #region Effects of altitude

    private void OnRefreshMoveSpeed(Entity<DragonFlightComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Altitude != FlightAltitude.High)
            return;

        args.ModifySpeed(ent.Comp.HighSpeedModifier, ent.Comp.HighSpeedModifier);
    }

    private void OnPickupAttempt(EntityUid uid, DragonFlightComponent comp, PickupAttemptEvent args)
    {
        if (comp.Altitude == FlightAltitude.High)
            args.Cancel();
    }

    private void OnInteractionAttempt(Entity<DragonFlightComponent> ent, ref InteractionAttemptEvent args)
    {
        if (ent.Comp.Altitude != FlightAltitude.High || args.Target is not { } target)
            return;

        // Things we are holding or carrying are fine; the ground below is out of reach.
        if (target == ent.Owner || _transform.IsParentOf(Transform(ent), target))
            return;

        args.Cancelled = true;
    }

    #endregion
}
