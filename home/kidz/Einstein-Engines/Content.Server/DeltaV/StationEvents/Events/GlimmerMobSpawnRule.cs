using System.Linq;
using Content.Server.GameTicking.Components;
using Robust.Shared.Random;
using Content.Server.GameTicking;
using Content.Server.NPC.Components;
using Content.Server.Psionics.Glimmer;
using Content.Server.StationEvents.Components;
using Content.Shared.Psionics.Glimmer;
using Content.Shared.Abilities.Psionics;

namespace Content.Server.StationEvents.Events;

public sealed class GlimmerMobRule : StationEventSystem<GlimmerMobRuleComponent>
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;


    protected override void Started(EntityUid uid, GlimmerMobRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var station = _gameTicker.GetSpawnableStations();
        if (station is null)
            return;

        var glimmerSources = new List<(GlimmerSourceComponent, TransformComponent)>();
        foreach (var source in EntityQuery<GlimmerSourceComponent, TransformComponent>().ToList())
        {
            if (!station.Contains(source.Item2.Owner))
                continue;

            glimmerSources.Add(source);
        }


        var normalSpawnLocations = new List<(VentCritterSpawnLocationComponent, TransformComponent)>();
        foreach (var source in EntityQuery<VentCritterSpawnLocationComponent, TransformComponent>().ToList())
        {
            if (!station.Contains(source.Item2.Owner))
                continue;

            normalSpawnLocations.Add(source);
        }

        var hiddenSpawnLocations = new List<(MidRoundAntagSpawnLocationComponent, TransformComponent)>();
        foreach (var source in EntityQuery<MidRoundAntagSpawnLocationComponent, TransformComponent>().ToList())
        {
            if (!station.Contains(source.Item2.Owner))
                continue;

            hiddenSpawnLocations.Add(source);
        }

        var baseCount = Math.Max(1, EntityQuery<PsionicComponent, NpcFactionMemberComponent>().Count() / 10);
        int multiplier = Math.Max(1, (int) _glimmerSystem.GetGlimmerTier() - 2);

        var total = baseCount * multiplier;

        int i = 0;
        while (i < total)
        {
            if (glimmerSources.Count != 0 && _robustRandom.Prob(0.4f))
            {
                Spawn(component.MobPrototype, _robustRandom.Pick(glimmerSources).Item2.Coordinates);
                i++;
                continue;
            }

            if (normalSpawnLocations.Count != 0)
            {
                Spawn(component.MobPrototype, _robustRandom.Pick(normalSpawnLocations).Item2.Coordinates);
                i++;
                continue;
            }

            if (hiddenSpawnLocations.Count != 0)
            {
                Spawn(component.MobPrototype, _robustRandom.Pick(hiddenSpawnLocations).Item2.Coordinates);
                i++;
                continue;
            }

            return;
        }
    }
}
