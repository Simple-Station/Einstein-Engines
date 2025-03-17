using Content.Server.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Gravity;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Projectiles;
using Robust.Shared.Audio;
using Robust.Shared.Map.Components;
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

    private void HighPressureMovements(Entity<GridAtmosphereComponent> gridAtmosphere,
        TileAtmosphere tile,
        EntityQuery<PhysicsComponent> bodies,
        EntityQuery<TransformComponent> xforms,
        EntityQuery<MovedByPressureComponent> pressureQuery,
        EntityQuery<MetaDataComponent> metas,
        EntityQuery<ProjectileComponent> projectileQuery,
        double gravity)
    {
        // No atmos yeets, return early.
        if (!SpaceWind
            || tile.PressureDirection is AtmosDirection.Invalid
            || tile.Air is null
            || !TryComp(gridAtmosphere.Owner, out MapGridComponent? mapGrid)
            || !_mapSystem.TryGetTileRef(gridAtmosphere.Owner, mapGrid, tile.GridIndices, out var tileRef))
            return;

        var tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];
        if (!tileDef.SimulatedTurf)
            return;

        var pressureVector = GetPressureVectorFromTile(gridAtmosphere, tile);
        if (!pressureVector.IsValid()
            || pressureVector.Length() <= 1) // Safeguard against "Extremely small vectors"
            return;

        pressureVector *= SpaceWindStrengthMultiplier;

        if (pressureVector.Length() > 15 && !tile.Hotspot.Valid)
        {
            if (_spaceWindSoundCooldown == 0 && !string.IsNullOrEmpty(SpaceWindSound))
            {
                var coordinates = _mapSystem.ToCenterCoordinates(tile.GridIndex, tile.GridIndices);
                _audio.PlayPvs(SpaceWindSound, coordinates, AudioParams.Default.WithVariation(0.125f).WithVolume(MathHelper.Clamp(pressureVector.Length() / 10, 10, 100)));
            }
        }

        if (_spaceWindSoundCooldown++ > SpaceWindSoundCooldownCycles)
            _spaceWindSoundCooldown = 0;

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
                tileDef,
                gravity,
                projectileQuery,
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
        ContentTileDefinition tile,
        double gravity,
        EntityQuery<ProjectileComponent> projectileQuery,
        TransformComponent? xform = null,
        PhysicsComponent? physics = null)
    {
        var (uid, component) = ent;
        if (!Resolve(uid, ref physics, false)
            || !Resolve(uid, ref xform)
            || physics.BodyType == BodyType.Static
            || float.IsPositiveInfinity(component.MoveResist)
            || physics.LinearVelocity.Length() >= SpaceWindMaxVelocity)
            return;

        var alwaysThrow = gravity == 0 || physics.BodyStatus == BodyStatus.InAir;
        // Coefficient of static friction in Newtons (kg * m/s^2), which might not apply under certain conditions.
        var coefficientOfFriction = gravity * physics.Mass * tile.MobFrictionNoInput;
        coefficientOfFriction *= _standingSystem.IsDown(uid) ? 3 : 1;

        if (HasComp<HumanoidAppearanceComponent>(ent))
            pressureVector *= HumanoidThrowMultiplier;
        var pVecLength = pressureVector.Length();
        if (!alwaysThrow && pVecLength < coefficientOfFriction)
            return;

        // As a first concession to Box2d being terrible at high speed collisions, we make sure the vector isn't faster than some speed limit.
        if (pVecLength >= SpaceWindMaxVelocity)
            pressureVector = pressureVector.Normalized() * SpaceWindMaxVelocity;

        // Yes this technically increases the magnitude by a small amount... I detest having to swap between "World" and "Local" vectors.
        // ThrowingSystem increments linear velocity by a given vector, but we have to do this anyways because reasons.
        var velocity = _transformSystem.GetWorldRotation(uid).ToWorldVec() + pressureVector;
        if ((velocity + physics.LinearVelocity).Length() >= SpaceWindMaxVelocity)
            return; // Still too fast. We must be going in *roughly* the same direction and at some high speed.
                    // There isn't actually a good way to tell this with any amount of precision and still have
                    // arbitrary radian throws. Or at least if there is, I don't know it yet.
                    // TODO: Maybe revisit this in the future --TCJ

        _sharedStunSystem.TryKnockdown(uid, TimeSpan.FromSeconds(SpaceWindKnockdownTime), false);
        _throwing.TryThrow(uid, velocity, physics, xform, projectileQuery,
            pVecLength, doSpin: physics.AngularVelocity < SpaceWindMaxAngularVelocity);
        component.LastHighPressureMovementAirCycle = cycle;
    }
}
