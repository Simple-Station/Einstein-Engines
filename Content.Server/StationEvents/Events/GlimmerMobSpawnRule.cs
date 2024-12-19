using System.Linq;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;
using Content.Server.GameTicking;
using Content.Server.NPC.Components;
using Content.Server.Psionics.Glimmer;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Abilities.Psionics;
using Content.Shared.NPC.Components;
using Robust.Shared.Map;

namespace Content.Server.StationEvents.Events;

public sealed class GlimmerMobRule : StationEventSystem<GlimmerMobRuleComponent>
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly StationSystem _stations = default!;

    protected override void Started(EntityUid uid, GlimmerMobRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var stations = _gameTicker.GetSpawnableStations();
        if (stations.Count <= 0)
            return;

        List<EntityCoordinates>
            glimmerSources = GetCoords<GlimmerSourceComponent>(stations),
            normalSpawns = GetCoords<VentCritterSpawnLocationComponent>(stations),
            hiddenSpawns = GetCoords<MidRoundAntagSpawnLocationComponent>(stations);

        var psionics = EntityQuery<PsionicComponent, NpcFactionMemberComponent>().Count();
        var baseCount = Math.Max(1, psionics / comp.MobsPerPsionic);
        var multiplier = Math.Max(1, (int) _glimmer.GetGlimmerTier() - (int) comp.GlimmerTier);
        var total = baseCount * multiplier;

        Log.Info($"Spawning {total} of {comp.MobPrototype} from {ToPrettyString(uid):rule}");

        for (var i = 0; i < total; i++)
        {
            // if we cant get a spawn just give up
            if (!TrySpawn(comp, glimmerSources, comp.GlimmerProb) &&
                !TrySpawn(comp, normalSpawns, comp.NormalProb) &&
                !TrySpawn(comp, hiddenSpawns, comp.HiddenProb))
                return;
        }
    }

    private List<EntityCoordinates> GetCoords<T>(List<EntityUid> allowedStations) where T : IComponent
    {
        var coords = new List<EntityCoordinates>();
        var query = EntityQueryEnumerator<T, TransformComponent>();

        while (query.MoveNext(out var uid, out _, out var xform))
        {
            var station = _stations.GetOwningStation(uid, xform);
            if (station is null || !allowedStations.Contains(station.Value))
                continue;

            coords.Add(xform.Coordinates);
        }

        return coords;
    }

    private bool TrySpawn(GlimmerMobRuleComponent comp, List<EntityCoordinates> spawns, float prob)
    {
        if (spawns.Count == 0 || !RobustRandom.Prob(prob))
            return false;

        var coords = RobustRandom.Pick(spawns);
        Spawn(comp.MobPrototype, coords);
        return true;
    }
}
