using System.Linq;
using System.Numerics;
using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.GameTicking;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Map;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Maps;
using Content.Server.Salvage;
using Content.Server.Salvage.Magnet;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.Coordinates;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents.Events;

public sealed class BluespaceErrorRule : StationEventSystem<BluespaceErrorRuleComponent>
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private List<(Entity<TransformComponent> Entity, EntityUid MapUid, Vector2 LocalPosition)> _playerMobs = new();

    protected override void Started(EntityUid uid, BluespaceErrorRuleComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var shuttleMap = _mapManager.CreateMap();

        if (!_map.TryLoadGrid(shuttleMap, new ResPath(component.GridPath), out var gridUids))
            return;
        component.GridUid = gridUids.Value.Owner;
        if (component.GridUid is not EntityUid gridUid)
            return;
        component.startingValue = _pricing.AppraiseGrid(gridUid);
        _shuttle.SetIFFColor(gridUid, component.Color);
        var offset = _random.NextVector2Box(component.minX, component.minY, component.maxX, component.maxY); // Hullrot - fix random event spawns being only around kal
        var mapId = GameTicker.DefaultMap;
        var mapUid = _mapManager.GetMapEntityId(mapId);
        if (TryComp<ShuttleComponent>(component.GridUid, out var shuttle))
        {
            _shuttle.FTLToCoordinates(gridUid, shuttle, new EntityCoordinates(mapUid, offset), 0f, 0f, 30f);
        }

        var stringEnd = component.GridPath.Length-1;
        var path = component.GridPath;
        var startingIndex = 0;
        while (path[stringEnd] != '/')
        {
            startingIndex = stringEnd;
            stringEnd--;
        }
        var GameProto= component.GridPath.Substring(startingIndex);
        // remove yaml shit
        GameProto = GameProto.Remove(GameProto.Length - 4);
        if (_prototypeManager.TryIndex<GameMapPrototype>(GameProto, out var stationProto))
        {
            _station.InitializeNewStation(stationProto.Stations[GameProto], new List<EntityUid>(){component.GridUid.Value});
        }


    }

    protected override void Ended(EntityUid uid, BluespaceErrorRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if(!EntityManager.TryGetComponent<TransformComponent>(component.GridUid, out var gridTransform))
        {
            Log.Error("bluespace error objective was missing transform component");
            return;
        }

        if (gridTransform.GridUid is not EntityUid gridUid)
        {
            Log.Error( "bluespace error has no associated grid?");
            return;
        }

        var gridValue = _pricing.AppraiseGrid(gridUid, null);

        var mobQuery = AllEntityQuery<HumanoidAppearanceComponent, MobStateComponent, TransformComponent>();
        _playerMobs.Clear();

        while (mobQuery.MoveNext(out var mobUid, out _, out _, out var xform))
        {
            if (xform.GridUid == null || xform.MapUid == null || xform.GridUid != gridUid)
                continue;

            // Can't parent directly to map as it runs grid traversal.
            _playerMobs.Add(((mobUid, xform), xform.MapUid.Value, _transform.GetWorldPosition(xform)));
            _transform.DetachParentToNull(mobUid, xform);
        }

        // Deletion has to happen before grid traversal re-parents players.
        Del(gridUid);

        foreach (var mob in _playerMobs)
        {
            _transform.SetCoordinates(mob.Entity.Owner, new EntityCoordinates(mob.MapUid, mob.LocalPosition));
        }


        var query = EntityQueryEnumerator<StationBankAccountComponent>();
        while(query.MoveNext(out var id, out var bank))
        {
            _cargo.UpdateBankAccount(id, bank,(int) (gridValue * component.RewardFactor) );
        }
    }
}

