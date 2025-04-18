using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Physics;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This system detects if an entity is standing on light.
/// It casts rays from the PointLight to the player.
/// </summary>
public sealed class LightDetectionSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    public override void Update(float frameTime)
    {

        // todo: make it check every 0.25s-1s
        var query = EntityQueryEnumerator<LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            DetectLight(uid, comp);
        }
    }

    private void DetectLight(EntityUid uid, LightDetectionComponent comp)
    {
        var xform = EntityManager.GetComponent<TransformComponent>(uid);
        var worldPos = _transformSystem.GetWorldPosition(uid);

        comp.IsOnLight = false;
        var query = EntityQueryEnumerator<PointLightComponent>();
        while (query.MoveNext(out var point, out var pointLight))
        {
            // todo: skip maplight because the system it doesn't work with glacier outside terrain
            if (!pointLight.Enabled)
                continue;

            var lightPos = _transformSystem.GetWorldPosition(point);
            var distance = (lightPos - worldPos).Length();

            if (distance <= 0.01f) // So the debug stops crashing
                continue;

            if (distance > pointLight.Radius)
                continue;

            var direction = (worldPos - lightPos).Normalized();
            var ray = new CollisionRay(lightPos, direction, (int)CollisionGroup.Opaque);


            var rayResults = _physicsSystem.IntersectRay(
                xform.MapID,
                ray,
                distance,
                point); // todo: remove this once slings get night vision action


            var hasBeenBlocked = false;
            foreach (var result in rayResults)
            {
                if (result.HitEntity != uid)
                {
                    hasBeenBlocked = true;
                    break;
                }
            }

            if (!hasBeenBlocked)
            {
                comp.IsOnLight = true;
                return;
            }
        }

    }
}
