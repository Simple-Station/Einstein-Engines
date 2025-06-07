using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.GameTicking;
using Content.Server.StationEvents.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random.Helpers;
using Content.Shared.Salvage;
using System.Linq;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;


namespace Content.Server.StationEvents.Events;

public sealed class PirateRadioSpawnRule : StationEventSystem<PirateRadioSpawnRuleComponent>
{
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _confMan = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;

    protected override void Started(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        SpawnListeningOutpost((uid, component), gameRule, args);
        SpawnDebris((uid, component));
    }

    private void SpawnListeningOutpost(Entity<PirateRadioSpawnRuleComponent> ent, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(ent.Owner, ent.Comp, gameRule, args);

        var station = _gameTicker.GetSpawnableStations();
        var stationGrids = new HashSet<EntityUid>();

        foreach (var stations in station)
        {
            if (TryComp<StationDataComponent>(stations, out var data) && _station.GetLargestGrid(data) is { } grid)
                stationGrids.Add(grid);
        }

        // _random forces Test Fails if given an empty list. which is guaranteed to happen during Tests.
        if (stationGrids.Count <= 0)
            return;

        var targetStation = _random.Pick(stationGrids);
        var targetMapId = Transform(targetStation).MapID;

        if (!_mapSystem.MapExists(targetMapId))
            return;

        var randomOffset = _random.NextVector2(ent.Comp.MinimumDistance, ent.Comp.MaximumDistance);
        var randomMap = _random.Pick(ent.Comp.PirateRadioShuttlePath);
        var randomMapPath = new ResPath(randomMap);
        var mapId = Transform(targetStation).MapID;

        if (!_map.TryLoadGrid(mapId, randomMapPath, out var outpostId, offset: randomOffset))
            return;
    }

    private void SpawnDebris(Entity<PirateRadioSpawnRuleComponent> ent)
    {
        if (_confMan.GetCVar(CCVars.WorldgenEnabled)
            || ent.Comp.DebrisCount <= 0)
            return;

        var outpostaabb = _xform.GetWorldPosition(ent.Owner);
        var k = 0;

        while (k < ent.Comp.DebrisCount)
        {
            var debrisRandomOffset = _random.NextVector2(ent.Comp.MinimumDebrisDistance, ent.Comp.MaximumDebrisDistance);
            var randomer = _random.NextVector2(ent.Comp.DebrisMinimumOffset, ent.Comp.DebrisMaximumOffset); //Second random vector to ensure the outpost isn't perfectly centered in the debris field

            var salvPrototypes = _prototypeManager.EnumeratePrototypes<SalvageMapPrototype>().ToList();
            var salvageProto = _random.Pick(salvPrototypes);

            if (!_mapSystem.MapExists(GameTicker.DefaultMap))
                return;

            // Round didn't start before running this, leading to a null-space test fail.
            if (GameTicker.DefaultMap == MapId.Nullspace)
                return;

            if (!_map.TryLoadGrid(
                GameTicker.DefaultMap,
                salvageProto.MapPath,
                out _,
                offset: outpostaabb + debrisRandomOffset + randomer)
            )
                return;

            k++;
        }
    }

    protected override void Ended(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (component.AdditionalRule != null)
            GameTicker.EndGameRule(component.AdditionalRule.Value);
    }
}
