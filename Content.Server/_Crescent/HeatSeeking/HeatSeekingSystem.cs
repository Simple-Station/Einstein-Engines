using System.Numerics;
using Content.Shared.Interaction;
using Content.Server.Shuttles.Components;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Server._Crescent.HeatSeeking;

/// <summary>
/// This handles...
/// </summary>
public sealed class HeatSeekingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly RotateToFaceSystem _rotate = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatSeekingComponent, TransformComponent>(); // get all heat seeking missiles
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (TryComp<ProjectileComponent>(uid, out var projectile) && TryComp<GunComponent>(projectile.Shooter, out var shooterGunComp))
                comp.InitialSpeed = shooterGunComp.ProjectileSpeed;

            if (comp.Speed < comp.InitialSpeed)
                comp.Speed = comp.InitialSpeed; // start at initial speed
            if (comp.Speed < comp.TopSpeed)
                comp.Speed += comp.Acceleration * frameTime; // accelerate to top speed once target is locked

            _physics.SetLinearVelocity(uid, _transform.GetWorldRotation(xform).ToWorldVec() * comp.Speed); // move missile forward at current speed

            if (comp.TargetEntity.HasValue) // if the missile has a target, run its guidance algorithm
            {
                if ((comp.GuidanceAlgorithm & GuidanceType.PredictiveGuidance) != 0) { PredictiveGuidance(uid, comp, xform, frameTime); }
                else if ((comp.GuidanceAlgorithm & GuidanceType.PurePursuit) != 0) { PurePursuit(uid, comp, xform, frameTime); }
                else { PredictiveGuidance(uid, comp, xform, frameTime); } // if yaml is invalid, default to Predictive Guidance
            }
            else
            {
                GetNewTarget(uid, comp, xform);
            }
        }
    }

    /// <summary>
    /// Gets the best valid target given the properties of a HeatSeekingComponent. Locks onto entities with the <see cref="CanBeHeatTrackedComponent"/> component.
    /// </summary>
    public void GetNewTarget(EntityUid uid, HeatSeekingComponent component, TransformComponent transform)
    {
        Angle closestAngle = 4;
        EntityUid? bestGrid = null;
        var shipQuery = EntityQueryEnumerator<CanBeHeatTrackedComponent, TransformComponent>(); // get all shuttle consoles
        while (shipQuery.MoveNext(out var shipUid, out var shipComp, out var shipXform)) // go through each existing thruster component to find the best valid target
        {
            var angle = (
                _transform.ToMapCoordinates(shipXform.Coordinates).Position -
                _transform.ToMapCoordinates(transform.Coordinates).Position
            ).ToWorldAngle(); // current angle towards target
            var distance = Vector2.Distance(
                _transform.ToMapCoordinates(transform.Coordinates).Position,
                _transform.ToMapCoordinates(shipXform.Coordinates).Position
            ); // current distance from target

            if (angle > _transform.GetWorldRotation(transform) + component.FOV / 2 * Math.PI / 180f
            || angle < _transform.GetWorldRotation(transform) - component.FOV / 2 * Math.PI / 180f) // if target is out of FOV, skip it.
            {
                continue;
            }
            if (distance > component.DefaultSeekingRange) // if target is out of range, skip it.
            {
                continue;
            }

            if (TryComp<ProjectileComponent>(uid, out var projectile) && TryComp<TransformComponent>(projectile.Shooter, out var shooterTransform)) // get the shooter of the missile
            {
                var shooterGridUid = shooterTransform.GridUid;
                if (TryComp<TransformComponent>(shipUid, out var hitTransform) && shooterGridUid == hitTransform.GridUid)
                {
                    continue;
                }
            }
            if (closestAngle > Math.Abs(angle - _transform.GetWorldRotation(transform) + distance / component.DefaultSeekingRange)) // if this target is the best target checked so far, save it.
            {
                closestAngle = Math.Abs(angle - _transform.GetWorldRotation(transform) + distance / component.DefaultSeekingRange);
                bestGrid = shipUid;
            }
        }
        if (bestGrid.HasValue) // after checking all valid targets, pick the best one.
        {
            component.TargetEntity = bestGrid;
        }
    }

    /// <summary>
    /// Attempts to predict the target's position at impact time.
    /// </summary>
    public void PredictiveGuidance(EntityUid uid, HeatSeekingComponent comp, TransformComponent xform, float frameTime)
    {
        if (comp.TargetEntity.HasValue)
        {
            float oldDistance = comp.oldDistance;
            Vector2 oldPosition = comp.oldPosition;
            var entXform = Transform(comp.TargetEntity.Value); // get target transform
            var distance = Vector2.Distance(
                _transform.ToMapCoordinates(xform.Coordinates).Position,
                _transform.ToMapCoordinates(entXform.Coordinates).Position
            ); // current distance from target
            var angle = (
                _transform.ToMapCoordinates(entXform.Coordinates).Position -
                _transform.ToMapCoordinates(xform.Coordinates).Position
            ).ToWorldAngle(); // current angle towards target

            if (angle > _transform.GetWorldRotation(xform) + comp.FOV * Math.PI / 180f
            || angle < _transform.GetWorldRotation(xform) - comp.FOV * Math.PI / 180f) // if missile missed, lose lock.
            {
                comp.TargetEntity = null;
                return;
            }

            var targetVelocity = _transform.ToMapCoordinates(entXform.Coordinates).Position - oldPosition; // get target velocity
            float timeToImpact = distance / (oldDistance - distance); // time it will take for the missile to reach the target
            if (timeToImpact < 0.1) { timeToImpact = 0.1f; } // prevent negative time to impact, that messes up guidance
            var predictedPosition = _transform.ToMapCoordinates(entXform.Coordinates).Position + (targetVelocity * timeToImpact); // predict target position at impact time

            Angle targetAngle = (predictedPosition - _transform.ToMapCoordinates(xform.Coordinates).Position).ToWorldAngle(); // the angle the missile will try to face

            _rotate.TryRotateTo(uid, targetAngle, frameTime, comp.WeaponArc, comp.RotationSpeed?.Theta ?? double.MaxValue, xform); // rotate towards target angle

            comp.oldPosition = _transform.ToMapCoordinates(entXform.Coordinates).Position;
            comp.oldDistance = distance;
        }
    }

    /// <summary>
    /// Aims the missile directly at target.
    /// </summary>
    public void PurePursuit(EntityUid uid, HeatSeekingComponent comp, TransformComponent xform, float frameTime)
    {
        if (comp.TargetEntity.HasValue)
        {
            var entXform = Transform(comp.TargetEntity.Value); // get target transform
            var originalAngle = _transform.GetWorldRotation(xform); // get current angle of missile

            var angle = (
                _transform.ToMapCoordinates(entXform.Coordinates).Position -
                _transform.ToMapCoordinates(xform.Coordinates).Position
            ).ToWorldAngle(); // current angle towards target

            _rotate.TryRotateTo(uid, angle, frameTime, comp.WeaponArc, comp.RotationSpeed?.Theta ?? double.MaxValue, xform); // rotate towards target angle
        }
    }
}
