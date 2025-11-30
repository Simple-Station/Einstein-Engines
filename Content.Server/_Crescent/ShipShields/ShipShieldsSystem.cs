using System.Numerics;
using Content.Shared._Crescent.ShipShields;
using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Server.GameStates;
using Content.Server.Power.Components;
using Robust.Shared.Physics;
using Content.Shared._Crescent.SpaceArtillery;
using Content.Shared.Projectiles;
using Content.Shared._RMC14.Weapons.Ranged.Prediction;


namespace Content.Server._Crescent.ShipShields;
public sealed partial class ShipShieldsSystem : EntitySystem
{
    private const string ShipShieldPrototype = "ShipShield";
    private const float Padding = 10f;
    private const float CollisionThreshold = 50f;
    //private const float DeflectionSpread = 25f;
    private const float EmitterUpdateRate = 1f; //mlg changed this to 1.5. setting it back to 1

    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    [Dependency] private readonly FixtureSystem _fixtureSystem = default!;

    [Dependency] private readonly PhysicsSystem _physicsSystem = default!;

    [Dependency] private readonly PvsOverrideSystem _pvsSys = default!;

    private ISawmill _sawmill = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ShipShieldEmitterComponent, ApcPowerReceiverComponent>();
        while (query.MoveNext(out var uid, out var emitter, out var power))
        {
            //.2 | 2025 here to untangle this mess

            // emitter only runs its code every EmitterUpdateRate. emitter.Accumulator just adds up to 1.5 each time and...
            emitter.Accumulator += frameTime;

            //cancels the whole op for this shield if it's not over 1.5. after it is, it resets.
            if (emitter.Accumulator < EmitterUpdateRate)
                continue;

            // if emitter.DamageIveTakenSoFar^emitter.DamageMultiplier is bigger than the power draw the emitter can take, then the emitter's down and is recharging
            // commented because shields draw a flat amount of power now
            // if ((float) Math.Pow(emitter.Damage, emitter.DamageExp) >= emitter.MaxDraw)
            //     emitter.Recharging = true;

            // OR if the emitter is not powered, then it must be down, and we should be recharging as soon as it wakes back up.
            // if (!power.Powered)
            //     emitter.Recharging = true;

            // if we're here, then .Accumulator >= 1.5. dunk it back down so we're ready to count up to 1.5 again.
            emitter.Accumulator -= EmitterUpdateRate;

            // OverloadAccumulator is over 0 when our shield's popped. it's effectively the timer counting down until the shield goes back up.
            if (emitter.OverloadAccumulator > 0)
            {
                emitter.OverloadAccumulator -= EmitterUpdateRate;
            }

            //this is the value that the emitter will heal up by each 1.5s, decided by .HealPerSecond. 
            float healed = emitter.HealPerSecond * EmitterUpdateRate;

            //if our shield is down/recharging, then we should heal it's hp back up by this amount.
            if (emitter.Recharging)
                healed *= emitter.UnpoweredBonus;

            emitter.Damage -= healed; //line of code that actually does the healing

            // if we healed, our damage taken is MOST LIKELY under 0. fix it by setting it to 0.
            if (emitter.Damage < 0)
            {
                emitter.Damage = 0;

                //if we reached this check, then our emitter must be fully healed and should be back on
                if (power.Powered)
                    emitter.Recharging = false;
            }

            //this adjusts how much power the emitter is drawing, each update tick. commented because we only draw a fixed amount now.
            //AdjustEmitterLoad(uid, emitter, power);

            //this checks if the shield the ship is attached to actually exists. if it's completely gone, don't bother calculating shields for this "grid"
            var parent = Transform(uid).GridUid;
            if (parent == null)
                return;

            // filter is needed to play the power down / power up noise for ONLY those on the ship grid
            var filter = _station.GetInOwningStation(uid);

            // if our emitter's .DamageTaken is over the .DamageLimit, then set the OverloadAccumulator to the DamageOverloadTimePunishment.
            if (emitter.Damage > emitter.DamageLimit)
                emitter.OverloadAccumulator = emitter.DamageOverloadTimePunishment;

            // if our shield is gone, AND the OverloadAccumulator is done counting down (with some padding), then...
            if (emitter.Shield is null && emitter.OverloadAccumulator < 1.5 && power.Powered) //put the shield back up!
            {
                emitter.Recharging = false; //stop boosting hp recharge now that it's up
                var shield = ShieldEntity(parent.Value, source: uid);
                if (shield != EntityUid.Invalid)
                {
                    emitter.Shield = shield;
                    emitter.Shielded = parent.Value;
                }
                _audio.PlayGlobal(emitter.PowerUpSound, filter, true, emitter.PowerUpSound.Params);
            }
            // if our emitter is Overloaded, AND the shield is active, shut down the shield.
            else if (emitter.OverloadAccumulator > 0 && emitter.Shield is not null)
            {
                emitter.Recharging = true; //boost hp recharge when it's down
                UnshieldEntity(parent.Value);
                emitter.Shield = null;
                emitter.Shielded = null;
                _audio.PlayGlobal(emitter.PowerDownSound, filter, true, emitter.PowerUpSound.Params);
            }

            if (!power.Powered && emitter.Shield is not null) // if shield is depowered then unshield the ship
            {
                emitter.Recharging = true; //boost hp recharge when it's down
                UnshieldEntity(parent.Value);
                emitter.Shield = null;
                emitter.Shielded = null;
                _audio.PlayGlobal(emitter.PowerDownSound, filter, true, emitter.PowerUpSound.Params);
            }

            // SHIELDS POWERING UP / DOWN BECAUSE OF POWER DRAW IS HANDLED SOMEWHERE ELSE

        }

        // better ways to do it but this will catch every edge case (probably)
        var cleanupQuery = EntityQueryEnumerator<ShipShieldedComponent>();
        while (cleanupQuery.MoveNext(out var uid, out var shieldedComp)) // five. hundred. entity queries.
        {
            if (!shieldedComp.Source.HasValue)
            {
                Del(uid);
            }
        }
    }
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShipShieldComponent, StartCollideEvent>(OnCollide);

        InitializeCommands();
        InitializeEmitters();
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("crescent.shipshields.server");
    }

    private void OnCollide(EntityUid uid, ShipShieldComponent component, StartCollideEvent args)
    {
        //_sawmill.Debug("collision detected, collided entity: " + args.OtherEntity.ToString());
        if (Transform(args.OtherEntity).Anchored)
            return;

        if (!TryComp<PhysicsComponent>(Transform(uid).GridUid, out var ourPhysics) || !TryComp<PhysicsComponent>(args.OtherEntity, out var theirPhysics))
            return;

        // only handle ship weapons for now. engine update introduced physics regressions. Let's polish everything else and circle back yeah?
        if (!HasComp<ShipWeaponProjectileComponent>(args.OtherEntity))
            return;

        if (HasComp<IgnoresHullrotShieldsComponent>(args.OtherEntity))
            return;

        if (!TryComp<ProjectileComponent>(args.OtherEntity, out var projectile))
            return;
        if (projectile.Weapon is not null)
        {
            // dont collide with projectiles coming from the same , grid  SPCR 2025
            if (component.Shielded == Transform(projectile.Weapon.Value).GridUid)
                return;
        }

        // .2 | 2025. this code used to make some projectiles ignore shields if their velocity was low enough.
        // i commented this out and replaced it by the IgnoresHullrotShields component.
        // var ourVelocity = ourPhysics.LinearVelocity;
        // var velocity = theirPhysics.LinearVelocity;

        // var collisionSpeedVector = Vector2.Subtract(ourVelocity, velocity);

        // if (Math.Abs(collisionSpeedVector.Length()) < CollisionThreshold)
        // {
        //     return;
        // }


        //if (TryComp<TimedDespawnComponent>(args.OtherEntity, out var despawn))
        //    despawn.Lifetime += despawn.Lifetime;

        // I originally tried reflection but the math is too hard with the fucked coordinate system in this game (WorldRotation can be negative. Vector to Angle conversion loses information. Etc etc.)
        // Might try again at some point using just vector math with this (https://math.stackexchange.com/questions/13261/how-to-get-a-reflection-vector)
        //var deflectionVector = Transform(args.OtherEntity).WorldPosition - Transform(uid).WorldPosition;
        //var angle = _random.NextFloat(DeflectionSpread);

        //if (_random.Prob(0.5f))
        //    angle = -angle;

        //deflectionVector = new Vector2((float) (Math.Cos(angle) * deflectionVector.X - Math.Sin(angle) * deflectionVector.Y), (float) (Math.Sin(angle) * deflectionVector.X - Math.Cos(angle) * deflectionVector.Y));

        // instead of reflecting the projectile, just delete it. this works better for gameplay and intuiting what is going on in a fight.
        //_gun.ShootProjectile(args.OtherEntity, deflectionVector, _physicsSystem.GetMapLinearVelocity(uid), uid, null, velocity.Length());

        if (component.Source != null)
        {
            //_sawmill.Debug("shield deflected projectile");
            var ev = new ShieldDeflectedEvent(args.OtherEntity);
            RaiseLocalEvent(component.Source.Value, ref ev);
        }
    }

    /// <summary>
    /// Produces a shield around a grid entity, if it doesn't already exist.
    /// </summary>
    /// <param name="entity">The entity being shielded.</param>
    /// <param name="mapGrid">The map grid component of the entity being shielded.</param>
    /// <param name="source">A shield generator or similar providing the shield for the entity</param>
    /// <returns>The shield entity.</returns>
    private EntityUid ShieldEntity(EntityUid entity, MapGridComponent? mapGrid = null, EntityUid? source = null)
    {
        if (TryComp<ShipShieldedComponent>(entity, out var existingShielded))
            return existingShielded.Shield;

        if (!Resolve(entity, ref mapGrid, false))
            return EntityUid.Invalid;

        var prototype = ShipShieldPrototype;

        var shield = Spawn(prototype, Transform(entity).Coordinates);
        var shieldPhysics = AddComp<PhysicsComponent>(shield);
        var shieldComp = EnsureComp<ShipShieldComponent>(shield);
        shieldComp.Shielded = entity;
        shieldComp.Source = source;

        _transformSystem.SetLocalPosition(shield, mapGrid.LocalAABB.Center);
        _transformSystem.SetWorldRotation(shield, _transformSystem.GetWorldRotation(entity));
        _transformSystem.SetParent(shield, entity);

        var chain = GenerateOvalFixture(shield, "shield", shieldPhysics, mapGrid);

        List<Vector2> roughPoly = new();

        var interval = chain.Count / PhysicsConstants.MaxPolygonVertices;

        int i = 0;

        while (i < PhysicsConstants.MaxPolygonVertices)
        {
            roughPoly.Add(chain.Vertices[i * interval]);
            i++;
        }

        var internalPoly = new PolygonShape();
        internalPoly.Set(roughPoly);

        _fixtureSystem.TryCreateFixture(shield, internalPoly, "internalShield",
            hard: false,
            collisionLayer: (int) CollisionGroup.FullTileLayer,
            body: shieldPhysics);

        _physicsSystem.WakeBody(shield, body: shieldPhysics);
        _physicsSystem.SetSleepingAllowed(shield, shieldPhysics, false);

        _pvsSys.AddGlobalOverride(shield);

        var shieldedComp = EnsureComp<ShipShieldedComponent>(entity);
        shieldedComp.Shield = shield;
        shieldedComp.Source = source;

        return shield;
    }

    private bool UnshieldEntity(EntityUid uid, ShipShieldedComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return false;

        Del(component.Shield);
        RemComp<ShipShieldedComponent>(uid);
        return true;
    }

    private ChainShape GenerateOvalFixture(EntityUid uid, string name, PhysicsComponent physics, MapGridComponent mapGrid, float padding = Padding)
    {
        float radius;
        float scale;
        var scaleX = true;

        var height = mapGrid.LocalAABB.Height + padding;
        var width = mapGrid.LocalAABB.Width + padding;

        if (width > height)
        {
            radius = 0.5f * height;
            scale = width / height;
        }
        else
        {
            radius = 0.5f * width;
            scale = height / width;
            scaleX = false;
        }

        var chain = new ChainShape();

        chain.CreateLoop(Vector2.Zero, radius);

        for (int i = 0; i < chain.Vertices.Length; i++)
        {
            if (scaleX)
            {
                chain.Vertices[i].X *= scale;
            }
            else
            {
                chain.Vertices[i].Y *= scale;
            }
        }

        _fixtureSystem.TryCreateFixture(uid, chain, name,
            hard: false,
            collisionLayer: (int) CollisionGroup.FullTileLayer,
            body: physics);

        return chain;
    }

    [ByRefEvent]
    public record struct ShieldDeflectedEvent(EntityUid Deflected)
    {

    }
}
