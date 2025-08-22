using Content.Shared.Projectiles;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Physics.Events;
using Content.Shared.Mobs;

namespace Content.Shared.Damage.Components;

public sealed class RequireProjectileTargetSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<RequireProjectileTargetComponent, PreventCollideEvent>(PreventCollide);
        SubscribeLocalEvent<RequireProjectileTargetComponent, MobStateChangedEvent>(CritBulletPass);
    }

    private void PreventCollide(Entity<RequireProjectileTargetComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (!ent.Comp.Active)
            return;

        var other = args.OtherEntity;
        if (HasComp<ProjectileComponent>(other) &&
            CompOrNull<TargetedProjectileComponent>(other)?.Target != ent)
        {
            args.Cancelled = true;
        }
    }

    private void SetActive(Entity<RequireProjectileTargetComponent> ent, bool value)
    {
        if (ent.Comp.Active == value)
            return;

        ent.Comp.Active = value;
        Dirty(ent);
    }
    private void CritBulletPass(Entity<RequireProjectileTargetComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.SoftCritical or MobState.Critical or MobState.Dead)
        {
            SetActive(ent, true);
        }
        else
        {
            SetActive(ent, false);
        }
    }
}
