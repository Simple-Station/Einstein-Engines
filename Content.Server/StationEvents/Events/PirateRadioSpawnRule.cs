using System.Linq;
using Content.Server.GameTicking;
using Content.Server.StationEvents.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.GameTicking.Components;
using Content.Shared.Salvage;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

public sealed class PirateRadioSpawnRule : StationEventSystem<PirateRadioSpawnRuleComponent>
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _confMan = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;

    protected override void Started(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

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

        var randomOffset = _random.NextVector2(component.MinimumDistance, component.MaximumDistance);
        if (!_mapLoader.TryLoadGrid(Transform(targetStation).MapID, _random.Pick(component.PirateRadioShuttlePath), out var outpostid, offset: _xform.GetWorldPosition(targetStation) + randomOffset))
            return;

        SpawnDebris(component, outpostid.Value.Owner);
    }

    private void SpawnDebris(PirateRadioSpawnRuleComponent component, EntityUid outpostid)
    {
        if (_confMan.GetCVar(CCVars.WorldgenEnabled)
            || component.DebrisCount <= 0)
            return;

        var outpostaabb = _xform.GetWorldPosition(outpostid);
        var k = 0;

        while (k < component.DebrisCount)
        {
            var debrisRandomOffset = _random.NextVector2(component.MinimumDebrisDistance, component.MaximumDebrisDistance);
            var randomer = _random.NextVector2(component.DebrisMinimumOffset, component.DebrisMaximumOffset); //Second random vector to ensure the outpost isn't perfectly centered in the debris field

            var salvPrototypes = _prototypeManager.EnumeratePrototypes<SalvageMapPrototype>().ToList();
            var salvageProto = _random.Pick(salvPrototypes);

            if (!_mapSystem.MapExists(GameTicker.DefaultMap))
                return;

            // Round didn't start before running this, leading to a null-space test fail.
            if (GameTicker.DefaultMap == MapId.Nullspace)
                return;

            if (!_mapLoader.TryLoadGrid(GameTicker.DefaultMap, salvageProto.MapPath, out _, offset: outpostaabb + debrisRandomOffset + randomer))
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
