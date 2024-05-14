using System.Linq;
using System.Numerics;
using System.Threading;
using Content.Server.Spawners.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Directions;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Random;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;
using Robust.Shared.Utility;

namespace Content.Server.Spawners.EntitySystems;

/// <summary>
/// Spawns entities in area around core, progressing from center to edges.
/// </summary>
public sealed class AreaSpawnerSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AreaSpawnerComponent, ComponentInit>(OnSpawnerInit);
        SubscribeLocalEvent<AreaSpawnerComponent, ComponentShutdown>(OnTimedSpawnerShutdown);
    }

    private void OnSpawnerInit(EntityUid uid, AreaSpawnerComponent component, ComponentInit args)
    {
        component.TokenSource?.Cancel();
        component.TokenSource = new CancellationTokenSource();
        uid.SpawnRepeatingTimer(TimeSpan.FromSeconds(component.IntervalSeconds), () => OnTimerFired(uid, component), component.TokenSource.Token);
    }

    /// <summary>
    /// If there is any space near the core, spawning entity, else increasing spawn radius, until it will be equal to radius in the component.
    /// </summary>
    /// <param name="uid">Entity uid</param>
    /// <param name="component">AreaSpawner component</param>
    private void OnTimerFired(EntityUid uid, AreaSpawnerComponent component)
    {
        var validTiles = GetValidTilesInRadius(uid, component, component.SpawnRadius);
        while (validTiles.Count == 0 && component.SpawnRadius <= component.Radius)
        {
            component.SpawnRadius++;
            validTiles = GetValidTilesInRadius(uid, component, component.SpawnRadius);
        }

        validTiles = GetValidTilesInRadius(uid, component, component.SpawnRadius);
        RandomlySpawnEntity(uid, component, validTiles);
    }

    /// <summary>
    /// Spawning entity in random chosen location from the list
    /// </summary>
    /// <param name="uid">Entity uid</param>
    /// <param name="component">AreaSpawner component</param>
    /// <param name="locations">List of possible location offsets from the core</param>
    public void RandomlySpawnEntity(EntityUid uid, AreaSpawnerComponent component, List<Vector2> locations)
    {
        if (component.SpawnToAllValidTiles)
        {
            foreach (var location in locations)
            {
                Spawn(component.SpawnPrototype, Transform(uid).Coordinates.Offset(location));
            }
        }
        else
        {
            var location = locations.ElementAt(_random.Next(0, locations.Count));
            Spawn(component.SpawnPrototype, Transform(uid).Coordinates.Offset(location));
        }

    }

    /// <summary>
    /// Getting valid for spawn tiles in radius
    /// </summary>
    /// <param name="uid">Entity uid</param>
    /// <param name="component">AreaSpawner component</param>
    /// <param name="radius">Radius in which we should search</param>
    /// <returns>List of location offsets from the core</returns>
    public List<Vector2> GetValidTilesInRadius(EntityUid uid, AreaSpawnerComponent component, int radius)
    {
        var validTiles = new List<Vector2>();
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                var tile = new Vector2(x, y);
                if (IsTileValidForSpawn(tile,
                        component.SpawnPrototype, uid))
                    validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    /// <summary>
    /// Checking if there is neither walls, nor same entity in the tile
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="entityPrototype"></param>
    /// <param name="uid"></param>
    /// <returns>True if tile is valid for spawn, else if not</returns>
    public bool IsTileValidForSpawn(Vector2 offset, string entityPrototype, EntityUid uid)
    {
        // Get the tile of spawn checker
        var xform = Transform(uid);
        var coords = xform.Coordinates.Offset(offset);
        var tile = coords.GetTileRef(EntityManager, _mapMan);
        if (tile == null)
            return false;

        // Check there are no walls there
        if (_turf.IsTileBlocked(tile.Value, CollisionGroup.Impassable))
        {
            return false;
        }

        // Check there are no same entities in tile
        foreach (var entity in _lookup.GetEntitiesInRange(coords, 0.1f))
        {
            if (Prototype(entity) == null)
                continue;
            if (Prototype(entity)!.ID == entityPrototype || Prototype(entity) == Prototype(uid))
                return false;
        }

        var offsets = new List<Vector2>();

        offsets.Add(new Vector2(0, 1));
        offsets.Add(new Vector2(0, -1));
        offsets.Add(new Vector2(1, 0));
        offsets.Add(new Vector2(-1, 0));

        foreach (var check in offsets)
        {
            foreach (var entity in _lookup.GetEntitiesInRange(coords.Offset(check), 0.1f))
            {
                if (Prototype(entity) == null)
                    continue;
                if (Prototype(entity)!.ID == entityPrototype || Prototype(entity) == Prototype(uid))
                    return true;
            }
        }


        return false;
    }

    private void OnTimedSpawnerShutdown(EntityUid uid, AreaSpawnerComponent component, ComponentShutdown args)
    {
        component.TokenSource?.Cancel();
    }
}
