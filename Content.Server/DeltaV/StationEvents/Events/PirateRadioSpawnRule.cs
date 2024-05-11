/*
* Delta-V - This file is licensed under AGPLv3
* Copyright (c) 2024 Delta-V Contributors
* See AGPLv3.txt for details.
*/

using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Server.Station.Components;
using Content.Shared.Salvage;
using Content.Shared.Random.Helpers;
using System.Linq;
using Content.Shared.CCVar;
using System.Numerics;

namespace Content.Server.StationEvents.Events;

public sealed class PirateRadioSpawnRule : StationEventSystem<PirateRadioSpawnRuleComponent>
{
    [Dependency] private readonly IEntityManager _entities = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _confMan = default!;

    protected override void Started(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        //Start of Syndicate Listening Outpost spawning system
        base.Started(uid, component, gameRule, args);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var aabbs = EntityQuery<StationDataComponent>().SelectMany(x =>
                x.Grids.Select(x =>
                    xformQuery.GetComponent(x).WorldMatrix.TransformBox(_mapManager.GetGridComp(x).LocalAABB)))
            .ToArray();

        var theta = _random.NextFloat(0, 2 * MathF.PI);
        var randomoffset = _random.NextFloat(800f, 1500f) * new Vector2(MathF.Cos(theta), MathF.Sin(theta));

        var outpostOptions = new MapLoadOptions
        {
            Offset = aabbs[0].Center + randomoffset,
            LoadMap = false,
        };
        if (!_map.TryLoad(GameTicker.DefaultMap, component.PirateRadioShuttlePath, out var outpostids, outpostOptions)) return;
        //End of Syndicate Listening Outpost spawning system

        //Start of Debris Field Generation
        var debrisSpawner = _confMan.GetCVar(CCVars.WorldgenEnabled);
        if (debrisSpawner == true || component.DebrisCount == 0) return;
        var debrisCount = Math.Clamp(component.DebrisCount, 0, 6);

        foreach (var id in outpostids)
        {
            if (!TryComp<MapGridComponent>(id, out var grid)) return;
            var outpostaabb = _entities.GetComponent<TransformComponent>(id).WorldMatrix.TransformBox(grid.LocalAABB);
            var alpha = _random.NextFloat(250f, 500f);
            var k = 1;
            while (k < debrisCount + 1)
            {
                var debrisOptions = new MapLoadOptions
                {
                    Offset = outpostaabb.Center + alpha * new Vector2(MathF.Cos(theta), MathF.Sin(theta)),
                    LoadMap = false,
                };

                var salvageProto = _random.Pick(_prototypeManager.EnumeratePrototypes<SalvageMapPrototype>().ToList());
                _map.TryLoad(GameTicker.DefaultMap, salvageProto.MapPath.ToString(), out _, debrisOptions);
                theta += _random.NextFloat(MathF.PI / 18, MathF.PI / 3);
                k++;
            }
        }
        //End of Debris Field generation
    }

    protected override void Ended(EntityUid uid, PirateRadioSpawnRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (component.AdditionalRule != null)
            GameTicker.EndGameRule(component.AdditionalRule.Value);
    }
}
