using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Throwing;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Player;
using System.Numerics;
using Content.Shared._White;
using Content.Shared.Standing;
using Robust.Shared.Physics.Components;

namespace Content.Shared._White.Grab;

public sealed class GrabThrownSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedLayingDownSystem _layingDown = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabThrownComponent, StartCollideEvent>(HandleCollide);
        SubscribeLocalEvent<GrabThrownComponent, StopThrowEvent>(OnStopThrow);
    }

    private void HandleCollide(Entity<GrabThrownComponent> ent, ref StartCollideEvent args)
    {
        if (_netMan.IsClient) // To avoid effect spam
            return;

        if (!HasComp<ThrownItemComponent>(ent))
        {
            RemComp<GrabThrownComponent>(ent);
            return;
        }

        if (ent.Comp.IgnoreEntity.Contains(args.OtherEntity))
            return;

        if (!HasComp<DamageableComponent>(ent))
            RemComp<GrabThrownComponent>(ent);

        if(!TryComp<PhysicsComponent>(ent, out var physicsComponent))
            return;

        ent.Comp.IgnoreEntity.Add(args.OtherEntity);

        var velocity = args.OurBody.LinearVelocity.Length();
        var velocitySquared = args.OurBody.LinearVelocity.LengthSquared();
        var mass = physicsComponent.Mass;
        var kineticEnergy = 0.5f * mass * velocitySquared;

        if (ent.Comp.StaminaDamageOnCollide != null)
            _stamina.TakeStaminaDamage(ent, ent.Comp.StaminaDamageOnCollide.Value);

        var kineticEnergyDamage = new DamageSpecifier();
        kineticEnergyDamage.DamageDict.Add("Blunt", 1);
        kineticEnergyDamage *= Math.Floor(kineticEnergy / 100) / 2 + 3;
        _damageable.TryChangeDamage(args.OtherEntity, kineticEnergyDamage);

        _layingDown.TryLieDown(args.OtherEntity, behavior: DropHeldItemsBehavior.AlwaysDrop);

        _color.RaiseEffect(Color.Red, new List<EntityUid>() { ent }, Filter.Pvs(ent, entityManager: EntityManager));
    }

    private void OnStopThrow(EntityUid uid, GrabThrownComponent comp, StopThrowEvent args)
    {
        if (comp.DamageOnCollide != null)
            _damageable.TryChangeDamage(uid, comp.DamageOnCollide);

        if (HasComp<GrabThrownComponent>(uid))
            RemComp<GrabThrownComponent>(uid);
    }

    /// <summary>
    /// Throwing entity to the direction and ensures GrabThrownComponent with params
    /// </summary>
    /// <param name="uid">Entity to throw</param>
    /// <param name="thrower">Entity that throws</param>
    /// <param name="vector">Direction</param>
    /// <param name="grabThrownSpeed">How fast you fly when thrown</param>
    /// <param name="staminaDamage">Stamina damage on collide</param>
    /// <param name="damageToUid">Damage to entity on collide</param>
    public void Throw(
        EntityUid uid,
        EntityUid thrower,
        Vector2 vector,
        float grabThrownSpeed,
        float? staminaDamage = null,
        DamageSpecifier? damageToUid = null)
    {
        var comp = EnsureComp<GrabThrownComponent>(uid);
        comp.StaminaDamageOnCollide = staminaDamage;
        comp.IgnoreEntity.Add(thrower);
        comp.DamageOnCollide = damageToUid;

        _layingDown.TryLieDown(uid, behavior: DropHeldItemsBehavior.AlwaysDrop);
        _throwing.TryThrow(uid, vector, grabThrownSpeed, animated: false);
    }
}
