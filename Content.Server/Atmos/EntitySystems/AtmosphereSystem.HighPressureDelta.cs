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
            || tile.PressureDirection is AtmosDirection.Invalid)
            return;

        // Previously, we were comparing against the square of the target mass. Now we are comparing smaller values over a variable length of time. TLDR: Smoother space wind
        var differentiatedPressure = 2 * tile.PressureDifference * frameTime * _cfg.GetCVar(CCVars.SpaceWindStrengthMultiplier);
        if (differentiatedPressure < SpaceWindMinimumCalculatedMass)
            return;
        // TODO ATMOS finish this

        // Don't play the space wind sound on tiles that are on fire...
        if (tile.PressureDifference > 15 && !tile.Hotspot.Valid)
        {
            if (_spaceWindSoundCooldown == 0 && !string.IsNullOrEmpty(SpaceWindSound))
            {
                var coordinates = _mapSystem.ToCenterCoordinates(tile.GridIndex, tile.GridIndices);
                _audio.PlayPvs(SpaceWindSound, coordinates, AudioParams.Default.WithVariation(0.125f).WithVolume(MathHelper.Clamp(tile.PressureDifference / 10, 10, 100)));
            }
        }


        if (tile.PressureDifference > 100)
        {
            // TODO ATMOS Do space wind graphics here!
        }

        if (_spaceWindSoundCooldown++ > SpaceWindSoundCooldownCycles)
            _spaceWindSoundCooldown = 0;

        // Used by ExperiencePressureDifference to correct push/throw directions from tile-relative to physics world.
        var gridWorldRotation = _transformSystem.GetWorldRotation(gridAtmosphere);

        // Atmos Directions only include NSEW cardinals, which means only 4 possible angles to throw at. If Monstermos is enabled, we'll instead do some
        // Vector shennanigans to smooth it out so that we can throw in increments of up to pi/32.
        var throwDirection = tile.PressureDirection.ToAngle().ToVec();
        if (MonstermosEqualization)
            foreach (var nextTile in tile.AdjacentTiles)
                if (nextTile is not null && nextTile.PressureDirection is not AtmosDirection.Invalid)
                    throwDirection += nextTile.PressureDirection.ToAngle().ToVec();

        // Before you ask, yes I did actually have to convert the angles to vectors, then add them together, then convert the end result back to a normalized vector.
        // We're normalizing this here and now so that we don't have to normalize it potentially hundreds of times during the next Foreach.
        var throwVector = (throwDirection.ToAngle() + gridWorldRotation).ToWorldVec().Normalized();

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
                differentiatedPressure,
                throwVector,
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

    public void ExperiencePressureDifference(
        Entity<MovedByPressureComponent> ent,
        int cycle,
        float pressureDifference,
        Vector2 throwVector,
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
            pressureDifference *= HumanoidThrowMultiplier;
        if (pressureDifference < physics.Mass)
            return;

        pressureDifference *= MathF.Max(physics.InvMass, SpaceWindMaximumCalculatedInverseMass);

        _throwing.TryThrow(uid, throwVector * MathF.Min(pressureDifference, SpaceWindMaxVelocity), pressureDifference);
        component.LastHighPressureMovementAirCycle = cycle;
    }
}
