using Content.Server.Shuttles.Events;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared._Lavaland.Shuttles.Components;
using Content.Shared._Lavaland.Shuttles.Systems;
using Content.Shared.Shuttles.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Linq;
using Content.Server.GameTicking;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Station.Events;
using System.Security.Principal;

namespace Content.Server._Lavaland.Shuttles.Systems;

public sealed class DockingShuttleSystem : SharedDockingShuttleSystem
{
    [Dependency] private readonly DockingConsoleSystem _console = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DockingShuttleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DockingShuttleComponent, FTLStartedEvent>(OnFTLStarted);
        SubscribeLocalEvent<DockingShuttleComponent, FTLCompletedEvent>(OnFTLCompleted);

        SubscribeLocalEvent<StationGridAddedEvent>(OnStationGridAdded);
        SubscribeLocalEvent<DockingShuttleComponent, ShuttleAddStationEvent>(OnAddStation);
    }

    private void OnMapInit(Entity<DockingShuttleComponent> ent, ref MapInitEvent args)
    {
        // add any whitelisted destinations that it can FTL to
        // since it needs a whitelist, this excludes the station
        var query = EntityQueryEnumerator<FTLDestinationComponent, MapComponent>();
        while (query.MoveNext(out var mapUid, out var dest, out var map))
        {
            if (!dest.Enabled || _whitelist.IsWhitelistFailOrNull(dest.Whitelist, ent))
                continue;

            AddDestinations(ent, map.MapId, mapUid);
        }

        // Also update all consoles
        var consoleQuery = EntityQueryEnumerator<DockingConsoleComponent>();
        while (consoleQuery.MoveNext(out var uid, out var dest))
        {
            if (TerminatingOrDeleted(uid))
                continue;

            _console.UpdateShuttle((uid, dest));
        }
    }

    private void OnFTLStarted(Entity<DockingShuttleComponent> ent, ref FTLStartedEvent args)
    {
        _console.UpdateConsolesUsing(ent);
    }

    private void OnFTLCompleted(Entity<DockingShuttleComponent> ent, ref FTLCompletedEvent args)
    {
        _console.UpdateConsolesUsing(ent);
    }


    /// <summary>
    /// When any station has been added or an item that has been added by a station it checks to see if it has the docking component.
    /// If there is a docking component then find the station that spawned it and add it to destinations.
    /// </summary>
    private void OnStationGridAdded(StationGridAddedEvent args)
    {
        var uid = args.GridId;
        if (!TryComp<DockingShuttleComponent>(uid, out var comp))
            return;

        // only add the destination once
        if (comp.Station != null)
            return;

        if (_station.GetOwningStation(uid) is not {} station || !TryComp<StationDataComponent>(station, out var data))
            return;

        // if this returns null. Suffer
        if (GetStationbyName(data, Name(station)) is not {} grid)
            return;

        // add the source station as a destination
        comp.Station = station;

        // Add the warp point and set the current location to the station uid
        AddDestinationUID(comp, Transform(uid).MapID, grid, "Station");
        RaiseLocalEvent(new OnStationGridAddedEvent(grid.Id));
    }

    /// <summary>
    /// If you have the exact station name then it will return it. This can also work with the ATS.
    /// </summary>
    private EntityUid? GetStationbyName(StationDataComponent component, string stationname)
    {
        foreach (var grid in component.Grids)
            if (stationname == Name(grid))
                return grid;

        return null;
    }

    /// <summary>
    /// If there is no mining shuttle on round start it will call this event and add it to destinations.
    /// </summary>
    private void OnAddStation(EntityUid uid, DockingShuttleComponent component, ShuttleAddStationEvent args)
    {
        component.Station = args.MapUid;
        AddDestinationUID(component, args.MapId, args.GridUid, "Station");
    }

    /// <summary>
    /// This function will specifically for lavaland components or station components on any given map.
    /// This will allow for you to add more maps or have many stations/lavaland structures to warp too :)
    /// </summary>
    private void AddDestinations(DockingShuttleComponent component, MapId map, EntityUid uid)
    {
        var planet = Name(uid).Split();
        // TODO: Change this to a switch/case if there is more station based components
        if (planet[0] != "Lavaland")
            AddStation(component, map);
        else
            AddLavalandStation(component, map);
    }

    /// <summary>
    /// Looks through all the BecomesStationComponent and adds said stations.
    /// If there is multiple station on the same map now it will create warp points for said stations
    /// </summary>
    private void AddStation(DockingShuttleComponent component, MapId map)
    {
        var query = EntityQueryEnumerator<BecomesStationComponent, TransformComponent>();
        while (query.MoveNext(out var gridUid, out var grid, out var xform))
        {
            if (xform.MapID != map)
                continue;

            // Check if this function is called again to update the shuttle console warp points.
            if (component.LocationUID.Contains(gridUid))
                continue;

            AddDestinationUID(component, map, gridUid, "Station");
        }
    }

    /// <summary>
    /// Will specifically look through lavaland stations to add all grids marked with lavalandstationcomponent
    /// This will allow people to add more warp points like a lavaland fight arena. :)
    /// </summary>
    private void AddLavalandStation(DockingShuttleComponent component, MapId map)
    {
        var query = EntityQueryEnumerator<LavalandStationComponent, TransformComponent>();
        while (query.MoveNext(out var gridUid, out var grid, out var xform))
        {
            // Check if this function is called again to update the shuttle console warp points.
            if (component.LocationUID.Contains(gridUid))
                continue;

            AddDestinationUID(component, map, gridUid, "Lavaland");
        }
    }

    /// <summary>
    /// Add the destination gridUID to the destinations.
    /// </summary>
    private void AddDestinationUID(DockingShuttleComponent component, MapId map, EntityUid gridUid, string? prefix = null)
    {
        var warppoint = Name(gridUid);
        if (prefix != null)
            warppoint = prefix + " - " + warppoint;

        component.Destinations.Add(new DockingDestination()
        {
            Name = warppoint,
            Map = map
        });
        component.LocationUID.Add(gridUid);
    }
}

public sealed class ShuttleAddStationEvent : EntityEventArgs
{
    public readonly EntityUid MapUid;
    public readonly MapId MapId;
    public readonly EntityUid GridUid;
    public ShuttleAddStationEvent(EntityUid mapUid, MapId mapId, EntityUid gridUid)
    {
        MapUid = mapUid;
        MapId = mapId;
        GridUid = gridUid;
    }
}
