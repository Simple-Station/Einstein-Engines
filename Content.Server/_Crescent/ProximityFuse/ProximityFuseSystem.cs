using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Components;
using System.Numerics;


namespace Content.Server._Crescent.ProximityFuse;

public sealed class ProximityFuseSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ProximityFuseComponent, TransformComponent>(); // get all proximity fuse components
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (!TryComp<ProjectileComponent>(uid, out var projectile) ||
                !TryComp<TransformComponent>(projectile.Shooter, out var shooterTransform))
                continue;

            if (comp.Safety > 0)
            {
                comp.Safety -= frameTime;
                continue;
            }

            var targetQuery = EntityQueryEnumerator<ProximityFuseTargetComponent, TransformComponent>();
            while (targetQuery.MoveNext(out var tuid, out var tcomp, out var txform))
            {
                float distance = Vector2.Distance(_transform.ToMapCoordinates(txform.Coordinates).Position, _transform.ToMapCoordinates(xform.Coordinates).Position);

                if (shooterTransform.GridUid == txform.GridUid)
                    continue;

                PhysicsComponent? theirPhysics = null;

                if (!TryComp<PhysicsComponent>(uid, out var ourPhysics) ||
                    (!TryComp(txform.GridUid, out theirPhysics) && !TryComp(tuid, out theirPhysics)))
                    return;

                // float nextDistance = Vector2.Distance(_transform.ToMapCoordinates(txform.Coordinates).Position + theirPhysics.LinearVelocity, _transform.ToMapCoordinates(xform.Coordinates).Position + ourPhysics.LinearVelocity);

                var t = comp.Targets.Find(x => x.ent == tuid);
                if (t != null && distance <= comp.MaxRange)
                {
                    t.LastDistance = t.Distance;
                    t.Distance = distance;
                    // if (t.Distance > t.LastDistance || t.Distance < nextDistance)
                    if (t.Distance > t.LastDistance)
                        Detonate(uid);
                }
                else if (t != null && distance > comp.MaxRange)
                    comp.Targets.Remove(t);
                else if (distance <= comp.MaxRange)
                    comp.Targets.Add(new Target() { ent = tuid, Distance = distance, LastDistance = distance });
            }
        }
    }
    /// <summary>
    /// Explodes the entity if it has an explosive component, otherwise, deletes the object
    /// </summary>
    public void Detonate(EntityUid uid)
    {
        if (TryComp<ExplosiveComponent>(uid, out var explosiveComp))
            _entMan.System<ExplosionSystem>().TriggerExplosive(uid);
        else
            _entMan.DeleteEntity(uid);
    }
}
