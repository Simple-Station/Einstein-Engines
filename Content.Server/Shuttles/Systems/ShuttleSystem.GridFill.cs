using System.Numerics;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Shared.Cargo.Components;
using Content.Shared.CCVar;
using Content.Shared.Random.Helpers;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private void InitializeGridFills()
    {
        SubscribeLocalEvent<GridSpawnComponent, StationPostInitEvent>(OnGridSpawnPostInit);
        SubscribeLocalEvent<StationCargoShuttleComponent, StationPostInitEvent>(OnCargoSpawnPostInit);

        SubscribeLocalEvent<GridFillComponent, MapInitEvent>(OnGridFillMapInit);

        Subs.CVar(_cfg, CCVars.GridFill, OnGridFillChange);
    }

    private void OnGridFillChange(bool obj)
    {
        // If you're doing this on live then god help you,
        if (obj)
        {
            var query = AllEntityQuery<GridSpawnComponent>();

            while (query.MoveNext(out var uid, out var comp))
            {
                GridSpawns(uid, comp);
            }

            var cargoQuery = AllEntityQuery<StationCargoShuttleComponent>();

            while (cargoQuery.MoveNext(out var uid, out var comp))
            {
                CargoSpawn(uid, comp);
            }
        }
    }

    private void OnGridSpawnPostInit(EntityUid uid, GridSpawnComponent component, ref StationPostInitEvent args)
    {
        GridSpawns(uid, component);
    }

    private void OnCargoSpawnPostInit(EntityUid uid, StationCargoShuttleComponent component, ref StationPostInitEvent args)
    {
        CargoSpawn(uid, component);
    }

    private void CargoSpawn(EntityUid uid, StationCargoShuttleComponent component)
    {
        if (!_cfg.GetCVar(CCVars.GridFill))
            return;

        if (!TryComp(uid, out StationDataComponent? dataComp))
            return;

        var targetGrid = _station.GetLargestGrid(dataComp);

        if (targetGrid == null)
            return;

        var mapId = _mapManager.CreateMap();

        if (_loader.TryLoadGrid(mapId, component.Path, out var ent))
        {
            if (HasComp<ShuttleComponent>(ent))
                TryFTLProximity(ent.Value, targetGrid.Value);

            _station.AddGridToStation(uid, ent.Value);
        }

        _mapSystem.DeleteMap(mapId);
    }

    private bool TryGridSpawn(EntityUid targetGrid, EntityUid stationUid, MapId mapId, GridSpawnGroup group, out EntityUid spawned)
    {
        spawned = EntityUid.Invalid;

        if (group.Paths.Count == 0)
        {
            Log.Error($"Found no paths for GridSpawn");
            return false;
        }

        var paths = new ValueList<ResPath>();

        // Round-robin so we try to avoid dupes where possible.
        if (paths.Count == 0)
        {
            paths.AddRange(group.Paths);
            _random.Shuffle(paths);
        }

        var path = paths[^1];
        paths.RemoveAt(paths.Count - 1);

        if (_loader.TryLoadGrid(mapId, path, out var grid))
        {
            if (HasComp<ShuttleComponent>(grid))
                TryFTLProximity(grid.Value, targetGrid);

            if (group.NameGrid)
            {
                var name = path.FilenameWithoutExtension;
                _metadata.SetEntityName(grid.Value, name);
            }

            spawned = grid.Value;
            return true;
        }

        Log.Error($"Error loading gridspawn for {ToPrettyString(stationUid)} / {path}");
        return false;
    }

    private void GridSpawns(EntityUid uid, GridSpawnComponent component)
    {
        if (!_cfg.GetCVar(CCVars.GridFill))
            return;

        if (!TryComp<StationDataComponent>(uid, out var data))
        {
            return;
        }

        var targetGrid = _station.GetLargestGrid(data);

        if (targetGrid == null)
            return;

        // Spawn on a dummy map and try to FTL if possible, otherwise dump it.
        var mapId = _mapManager.CreateMap();
        var valid = true;
        var paths = new List<ResPath>();

        foreach (var group in component.Groups.Values)
        {
            if (group.Paths.Count == 0)
            {
                Log.Error($"Found no paths for GridSpawn");
                continue;
            }

            var count = _random.Next(group.MinCount, group.MaxCount);
            paths.Clear();

            for (var i = 0; i < count; i++)
            {
                // Round-robin so we try to avoid dupes where possible.
                if (paths.Count == 0)
                {
                    paths.AddRange(group.Paths);
                    _random.Shuffle(paths);
                }

                var path = paths[^1];
                paths.RemoveAt(paths.Count - 1);

                if (_loader.TryLoadGrid(mapId, path, out var ent))
                {
                    if (TryComp<ShuttleComponent>(ent.Value, out var shuttle))
                    {
                        TryFTLProximity(ent.Value, targetGrid.Value);
                    }
                    else
                    {
                        valid = false;
                    }

                    if (group.Hide)
                    {
                        var iffComp = EnsureComp<IFFComponent>(ent.Value);
                        iffComp.Flags |= IFFFlags.HideLabel;
                        Dirty(ent.Value, iffComp);
                    }

                    if (group.StationGrid)
                    {
                        _station.AddGridToStation(uid, ent.Value);
                    }

                    if (group.NameGrid)
                    {
                        var name = path.FilenameWithoutExtension;
                        _metadata.SetEntityName(ent.Value, name);
                    }

                    foreach (var compReg in group.AddComponents.Values)
                    {
                        var compType = compReg.Component.GetType();

                        if (HasComp(ent.Value, compType))
                            continue;

                        var comp = _factory.GetComponent(compType);
                        AddComp(ent.Value, comp, true);
                    }
                }
                else
                {
                    valid = false;
                }

                if (!valid)
                {
                    Log.Error($"Error loading gridspawn for {ToPrettyString(uid)} / {path}");
                }
            }
        }

        _mapSystem.DeleteMap(mapId);
    }

    private void OnGridFillMapInit(EntityUid uid, GridFillComponent component, MapInitEvent args)
    {
        if (!_cfg.GetCVar(CCVars.GridFill))
            return;

        if (!TryComp<DockingComponent>(uid, out var dock) ||
            !TryComp<TransformComponent>(uid, out var xform) ||
            xform.GridUid == null)
        {
            return;
        }

        // Spawn on a dummy map and try to dock if possible, otherwise dump it.
        var mapId = _mapManager.CreateMap();
        var valid = false;

        if (_loader.TryLoadGrid(mapId, component.Path, out var grid))
        {
            var escape = GetSingleDock(grid.Value);

            if (escape != null)
            {
                var config = _dockSystem.GetDockingConfig(grid.Value, xform.GridUid.Value, escape.Value.Entity, escape.Value.Component, uid, dock);

                if (config != null)
                {
                    var shuttleXform = Transform(grid.Value);
                    FTLDock((grid.Value, shuttleXform), config);

                    if (TryComp<StationMemberComponent>(xform.GridUid, out var stationMember))
                    {
                        _station.AddGridToStation(stationMember.Station, grid.Value);
                    }

                    valid = true;
                }
            }

            foreach (var compReg in component.AddComponents.Values)
            {
                var compType = compReg.Component.GetType();

                if (HasComp(grid.Value, compType))
                    continue;

                var comp = _factory.GetComponent(compType);
                AddComp(grid.Value, comp, true);
            }
        }

        if (!valid)
        {
            Log.Error($"Error loading gridfill dock for {ToPrettyString(uid)} / {component.Path}");
        }

        _mapSystem.DeleteMap(mapId);
    }

    private (EntityUid Entity, DockingComponent Component)? GetSingleDock(EntityUid uid)
    {
        var dockQuery = GetEntityQuery<DockingComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(uid);

        var rator = xform.ChildEnumerator;

        while (rator.MoveNext(out var child))
        {
            if (!dockQuery.TryGetComponent(child, out var dock))
                continue;

            return (child, dock);
        }

        return null;
    }
}
