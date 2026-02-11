using Content.Server._White.GameTicking.Aspects.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;

namespace Content.Server._White.GameTicking.Aspects;

public sealed class SpawnAspect : AspectSystem<SpawnAspectComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Added(EntityUid uid, SpawnAspectComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        SpawnPresents(component);
    }

    private void SpawnPresents(SpawnAspectComponent component)
    {
        var minPresents = _random.Next(component.Min, component.Max);

        for (var i = 0; i < minPresents; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var targetCoords))
                break;

            EntityManager.SpawnEntity(component.Prototype, targetCoords);
        }
    }
}
