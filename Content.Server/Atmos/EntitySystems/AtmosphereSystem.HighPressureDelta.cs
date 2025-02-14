using Content.Server.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.CCVar;
using Content.Shared.Humanoid;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Server.Atmos.EntitySystems;

public sealed partial class AtmosphereSystem
{
    private const int SpaceWindSoundCooldownCycles = 75;

    private int _spaceWindSoundCooldown = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public string? SpaceWindSound { get; private set; } = "/Audio/Effects/space_wind.ogg";

    private readonly HashSet<Entity<MovedByPressureComponent>> _activePressures = new(8);

    private void UpdateHighPressure(float frameTime)
    {
        var toRemove = new RemQueue<Entity<MovedByPressureComponent>>();

        foreach (var ent in _activePressures)
        {
            var (uid, comp) = ent;
            MetaDataComponent? metadata = null;

            if (Deleted(uid, metadata))
            {
                toRemove.Add((uid, comp));
                continue;
            }

            if (Paused(uid, metadata))
                continue;

            comp.Accumulator += frameTime;

            if (comp.Accumulator < 2f)
                continue;

            // Reset it just for VV reasons even though it doesn't matter
            comp.Accumulator = 0f;
            toRemove.Add(ent);

            if (TryComp<PhysicsComponent>(uid, out var body))
            {
                _physics.SetBodyStatus(uid, body, BodyStatus.OnGround);
            }

            if (TryComp<FixturesComponent>(uid, out var fixtures))
            {
                foreach (var (id, fixture) in fixtures.Fixtures)
                {
                    _physics.AddCollisionMask(uid, id, fixture, (int) CollisionGroup.TableLayer, manager: fixtures);
                }
            }
        }

        foreach (var comp in toRemove)
        {
            _activePressures.Remove(comp);
        }
    }

    private void HighPressureMovements(Entity<GridAtmosphereComponent> gridAtmosphere, TileAtmosphere tile, EntityQuery<PhysicsComponent> bodies, EntityQuery<TransformComponent> xforms, EntityQuery<MovedByPressureComponent> pressureQuery, EntityQuery<MetaDataComponent> metas, float frameTime)
    {
        // No atmos yeets, return early.
        if (!SpaceWind
            || tile.PressureDirection is AtmosDirection.Invalid
            || tile.Air is null)
            return;

        var pressureVector = GetPressureVectorFromTile(gridAtmosphere, tile, frameTime);
        if (pressureVector.Length() < SpaceWindMinimumCalculatedMass)
            return;

        _entSet.Clear();
        _lookup.GetLocalEntitiesIntersecting(tile.GridIndex, tile.GridIndices, _entSet, 0f);

        foreach (var entity in _entSet)
        {
            // Ideally containers would have their own EntityQuery internally or something given recursively it may need to slam GetComp<T> anyway.
            // Also, don't care about static bodies (but also due to collisionwakestate can't query dynamic directly atm).
            if (!bodies.TryGetComponent(entity, out var body)
                || !pressureQuery.TryGetComponent(entity, out var pressure)
                || !pressure.Enabled
                || _containers.IsEntityInContainer(entity, metas.GetComponent(entity))
                || pressure.LastHighPressureMovementAirCycle >= gridAtmosphere.Comp.UpdateCounter)
                continue;

            // tl;dr YEET
            ExperiencePressureDifference(
                (entity, EnsureComp<MovedByPressureComponent>(entity)),
                gridAtmosphere.Comp.UpdateCounter,
                pressureVector,
                xforms.GetComponent(entity),
                body);
        }
    }

    // Called from AtmosphereSystem.LINDA.cs with SpaceWind CVar check handled there.
    private void ConsiderPressureDifference(GridAtmosphereComponent gridAtmosphere, TileAtmosphere tile, AtmosDirection differenceDirection, float difference)
    {
        gridAtmosphere.HighPressureDelta.Add(tile);

        if (difference <= tile.PressureDifference)
            return;

        tile.PressureDifference = difference;
        tile.PressureDirection = differenceDirection;
    }

    public void ExperiencePressureDifference(Entity<MovedByPressureComponent> ent,
        int cycle,
        Vector2 pressureVector,
        TransformComponent? xform = null,
        PhysicsComponent? physics = null)
    {
        var (uid, component) = ent;
        if (!Resolve(uid, ref physics, false)
            || !Resolve(uid, ref xform)
            || physics.BodyType == BodyType.Static
            || float.IsPositiveInfinity(component.MoveResist))
            return;

        if (HasComp<HumanoidAppearanceComponent>(ent))
            pressureVector *= HumanoidThrowMultiplier;
        if (pressureVector.Length() < physics.Mass)
            return;

        pressureVector *= MathF.Max(physics.InvMass, SpaceWindMaximumCalculatedInverseMass);
        var pressureTarget = pressureVector;
        if (pressureTarget.Length() > SpaceWindMaxVelocity)
            pressureTarget = pressureTarget.Normalized() * SpaceWindMaxVelocity;

        _throwing.TryThrow(uid, pressureVector.Length() > SpaceWindMaxVelocity
            ? pressureTarget.Normalized() * SpaceWindMaxVelocity
            : pressureVector,
            pressureVector.Length());
        component.LastHighPressureMovementAirCycle = cycle;
    }
}
