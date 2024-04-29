using Content.Server.Atmos.Components;
using Content.Shared.Atmos;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;

namespace Content.Server.Atmos.EntitySystems
{
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
                    _physics.SetBodyStatus(body, BodyStatus.OnGround);
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

        private void AddMovedByPressure(EntityUid uid, MovedByPressureComponent component, PhysicsComponent body)
        {
            if (!TryComp<FixturesComponent>(uid, out var fixtures))
                return;

            _physics.SetBodyStatus(body, BodyStatus.InAir);

            foreach (var (id, fixture) in fixtures.Fixtures)
            {
                _physics.RemoveCollisionMask(uid, id, fixture, (int) CollisionGroup.TableLayer, manager: fixtures);
            }

            // TODO: Make them dynamic type? Ehh but they still want movement so uhh make it non-predicted like weightless?
            // idk it's hard.

            component.Accumulator = 0f;
            _activePressures.Add((uid, component));
        }

        private void HighPressureMovements(Entity<GridAtmosphereComponent> gridAtmosphere, TileAtmosphere tile, EntityQuery<PhysicsComponent> bodies, EntityQuery<TransformComponent> xforms, EntityQuery<MovedByPressureComponent> pressureQuery, EntityQuery<MetaDataComponent> metas)
        {
            // TODO ATMOS finish this

            // Don't play the space wind sound on tiles that are on fire...
            if(tile.PressureDifference > 15 && !tile.Hotspot.Valid)
            {
                if(_spaceWindSoundCooldown == 0 && !string.IsNullOrEmpty(SpaceWindSound))
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

            // No atmos yeets, return early.
            if (!SpaceWind)
                return;

            // Used by ExperiencePressureDifference to correct push/throw directions from tile-relative to physics world.
            var gridWorldRotation = xforms.GetComponent(gridAtmosphere).WorldRotation;

            // If we're using monstermos, smooth out the yeet direction to follow the flow
            if (MonstermosEqualization)
            {
                // We step through tiles according to the pressure direction on the current tile.
                // The goal is to get a general direction of the airflow in the area.
                // 3 is the magic number - enough to go around corners, but not U-turns.
                var curTile = tile;
                for (var i = 0; i < 3; i++)
                {
                    if (curTile.PressureDirection == AtmosDirection.Invalid
                        || !curTile.AdjacentBits.IsFlagSet(curTile.PressureDirection))
                        break;
                    curTile = curTile.AdjacentTiles[curTile.PressureDirection.ToIndex()]!;
                }

                if (curTile != tile)
                    tile.PressureSpecificTarget = curTile;
            }

            _entSet.Clear();
            _lookup.GetLocalEntitiesIntersecting(tile.GridIndex, tile.GridIndices, _entSet, 0f);

            foreach (var entity in _entSet)
            {
                // Ideally containers would have their own EntityQuery internally or something given recursively it may need to slam GetComp<T> anyway.
                // Also, don't care about static bodies (but also due to collisionwakestate can't query dynamic directly atm).
                if (!bodies.TryGetComponent(entity, out var body) ||
                    !pressureQuery.TryGetComponent(entity, out var pressure) ||
                    !pressure.Enabled)
                    continue;

                if (_containers.IsEntityInContainer(entity, metas.GetComponent(entity))) continue;

                var pressureMovements = EnsureComp<MovedByPressureComponent>(entity);
                if (pressure.LastHighPressureMovementAirCycle < gridAtmosphere.Comp.UpdateCounter)
                {
                    // tl;dr YEET
                    ExperiencePressureDifference(
                        (entity, pressureMovements),
                        gridAtmosphere.Comp.UpdateCounter,
                        tile.PressureDifference,
                        tile.PressureDirection,
                        tile.PressureSpecificTarget != null ? _mapSystem.ToCenterCoordinates(tile.GridIndex, tile.PressureSpecificTarget.GridIndices) : EntityCoordinates.Invalid,
                        gridWorldRotation,
                        xforms.GetComponent(entity),
                        body);
                }
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

        //The EE version of this function drops pressureResistanceProbDelta, since it's not needed. If you are for whatever reason calling this function
        //And it isn't working, you've probably still got the ResistancePobDelta line included.
        public void ExperiencePressureDifference(
            Entity<MovedByPressureComponent> ent,
            int cycle,
            float pressureDifference,
            AtmosDirection direction,
            EntityCoordinates throwTarget,
            Angle gridWorldRotation,
            TransformComponent? xform = null,
            PhysicsComponent? physics = null)
        {
            var (uid, component) = ent;
            if (!Resolve(uid, ref physics, false))
                return;

            if (!Resolve(uid, ref xform))
                return;

            // Can we yeet the thing (due to probability, strength, etc.)
            if (physics.BodyType != BodyType.Static
                && !float.IsPositiveInfinity(component.MoveResist)
                && physics.Mass != 0)
            {
                var moveForce = pressureDifference / physics.Mass;

                if (moveForce > physics.Mass)
                {
                    AddMovedByPressure(uid, component, physics);
                    // Grid-rotation adjusted direction
                    var dirVec = (direction.ToAngle() + gridWorldRotation).ToWorldVec();

                    // TODO: Technically these directions won't be correct but uhh I'm just here for optimisations buddy not to fix my old bugs.
                    if (throwTarget != EntityCoordinates.Invalid)
                    {
                        var pos = ((throwTarget.ToMap(EntityManager).Position - xform.WorldPosition).Normalized() + dirVec).Normalized();
                        _physics.ApplyLinearImpulse(uid, pos * moveForce, body: physics);
                    }
                    else
                    {
                        _physics.ApplyLinearImpulse(uid, dirVec * moveForce, body: physics);
                    }

                    component.LastHighPressureMovementAirCycle = cycle;
                }
            }
        }
    }
}
