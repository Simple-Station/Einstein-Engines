using Content.Server._Crescent.Hullmods;
using Content.Server.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Factory.Components;
using Content.Server.PointCannons;
using Content.Shared._Crescent.Hardpoints;
using Content.Shared.Construction.Components;
using Content.Shared.PointCannons;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Timing;

namespace Content.Server._Crescent.Hardpoint;

/// <summary>
/// This handles...
/// </summary>
public sealed class HardpointSystem : SharedHardpointSystem
{
    [Dependency] private readonly PointCannonSystem _cannonSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    // Explosions can cause a lot of lookups and events to fire. So we time-limit it based on grids
    private const float UpdateDelay = 60f;
    private float InternalTimer = 0f;
    private HashSet<EntityUid> QueuedGrids = new();
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<FixturesComponent, AnchorStateChangedEvent>(OnFixtureAnchor);
        SubscribeLocalEvent<HardpointComponent, HardpointCannonAnchoredEvent>(OnCannonAnchor);
        SubscribeLocalEvent<HardpointComponent, HardpointCannonDeanchoredEvent>(OnCannonDeanchor);
        SubscribeLocalEvent<HardpointFixedMountComponent, SignalReceivedEvent>(OnSignalReceived);
    }
    private void OnSignalReceived(EntityUid uid, HardpointFixedMountComponent component, ref SignalReceivedEvent args)
    {
        if (!TryComp<HardpointComponent>(uid, out var hard))
            return;
        if (hard.anchoring is null)
            return;
        if (!TryComp<GunComponent>(hard.anchoring.Value, out var gun))
            return;

        var gridUid = Transform(uid).GridUid;
        if (gridUid != null && HasComp<PacifistShipHullmodComponent>(gridUid))
        {
                return;
        }

        if (args.Port == component.Trigger)
            _gun.AttemptShoot(hard.anchoring.Value, gun);
        var autoShoot = EnsureComp<AutoShootGunComponent>(hard.anchoring.Value);
        if (args.Port == component.Toggle)
            _gun.SetEnabled(hard.anchoring.Value, autoShoot, !autoShoot.Enabled);
    }

    public void OnFixtureAnchor(EntityUid uid, FixturesComponent comp, ref AnchorStateChangedEvent args)
    {
        if (args.Transform.GridUid is null)
            return;
        QueueHardpointRefresh(args.Transform.GridUid.Value);
    }

    public void QueueHardpointRefresh(EntityUid grid)
    {
        if (QueuedGrids.Contains(grid))
            return;
        QueuedGrids.Add(grid);
    }


    public void OnCannonAnchor(EntityUid uid, HardpointComponent comp, ref HardpointCannonAnchoredEvent args)
    {
        // This is just for turret-cannons!
        if (!TryComp<PointCannonComponent>(args.cannonUid, out var compx))
            return;
        _cannonSystem.LinkCannonToAllConsoles(args.cannonUid);
        QueueHardpointRefresh(args.gridUid);
    }

    public void OnCannonDeanchor(EntityUid uid, HardpointComponent comp, ref HardpointCannonDeanchoredEvent args)
    {
        // This is just for turret-cannons!
        if (!TryComp<PointCannonComponent>(args.CannonUid, out var compx))
            return;
        _cannonSystem.UnlinkCannon(args.CannonUid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        InternalTimer += frameTime;
        if (InternalTimer < UpdateDelay)
            return;
        InternalTimer = 0;
        EntityQuery<HardpointComponent> hardpointQuery = GetEntityQuery<HardpointComponent>();
        foreach(var grid in QueuedGrids)
        {
            if (TerminatingOrDeleted(grid))
            {
                QueuedGrids.Remove(grid);
                continue;
            }
            HashSet<Entity<HardpointComponent>> lookupList = new();
            _lookupSystem.GetGridEntities(grid, lookupList);
            foreach (var entity in lookupList)
            {
                if (entity.Comp.anchoring is null)
                    continue;
                if (TerminatingOrDeleted(entity.Comp.anchoring.Value))
                    continue;
                // This is just for turret-cannons!
                if (!TryComp<PointCannonComponent>(entity.Comp.anchoring.Value, out var compx))
                    continue;
                _cannonSystem.RefreshFiringRanges(entity.Comp.anchoring.Value, null, null, compx, entity.Comp.CannonRangeCheckRange);
            }
        }
        QueuedGrids.Clear();
    }
}
