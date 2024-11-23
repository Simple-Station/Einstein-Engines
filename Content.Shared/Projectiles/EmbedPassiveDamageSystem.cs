using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Timing;

namespace Content.Shared.Projectiles;

public sealed class EmbedPassiveDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmbedPassiveDamageComponent, EmbedEvent>(OnEmbed);
        SubscribeLocalEvent<EmbedPassiveDamageComponent, RemoveEmbedEvent>(OnRemoveEmbed);
    }

    private void OnEmbed(EntityUid uid, EmbedPassiveDamageComponent component, EmbedEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Embedded) || !TryComp<DamageableComponent>(args.Embedded, out var damageable))
            return;

        component.Embedded = args.Embedded;
        component.EmbeddedDamageable = damageable;
        component.NextDamage = _timing.CurTime + TimeSpan.FromSeconds(1f);
    }

    private void OnRemoveEmbed(EntityUid uid, EmbedPassiveDamageComponent component, RemoveEmbedEvent args)
    {
        component.Embedded = null;
        component.EmbeddedDamageable = null;
        component.NextDamage = TimeSpan.Zero;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<EmbedPassiveDamageComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Embedded is null ||
                comp.EmbeddedDamageable is null ||
                comp.NextDamage > curTime || // Make sure they're up for a damage tick
                comp.DamageCap != 0 && comp.EmbeddedDamageable.TotalDamage >= comp.DamageCap)
                continue;

            comp.NextDamage = curTime + TimeSpan.FromSeconds(1f);

            _damageable.TryChangeDamage(comp.Embedded, comp.Damage, false, false, comp.EmbeddedDamageable);
        }
    }
}
