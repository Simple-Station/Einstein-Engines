using Content.Shared._Shitmed.BodyEffects.Subsystems;
using Content.Shared.Body.Organ;
using Content.Shared.StatusEffect;
using Robust.Shared.Timing;
using Robust.Shared.Random;

namespace Content.Server._Shitmed.BodyEffects.Subsystems;

public sealed class RandomStatusActivationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _effects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomStatusActivationComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, RandomStatusActivationComponent component, ComponentInit args) => GetRandomTime(component);
    private void GetRandomTime(RandomStatusActivationComponent component)
    {
        var minTime = component.MinActivationTime;
        var maxTime = component.MaxActivationTime;
        var randomSeconds = _random.NextDouble() * (maxTime - minTime).TotalSeconds;
        var randomSpan = TimeSpan.FromSeconds(randomSeconds);
        component.NextUpdate = _timing.CurTime + minTime + randomSpan;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RandomStatusActivationComponent>();
        var now = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            if (!TryComp<StatusEffectsComponent>(uid, out var effects))
                continue;

            foreach (var (key, component) in comp.StatusEffects)
                _effects.TryAddStatusEffect(uid, key, comp.Duration ?? TimeSpan.FromSeconds(1), refresh: true, component, effects);

            GetRandomTime(comp);
        }
    }
}
