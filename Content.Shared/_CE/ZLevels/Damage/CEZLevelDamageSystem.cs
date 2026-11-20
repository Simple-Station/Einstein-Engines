/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Effects;
using Content.Shared.Stunnable;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._CE.ZLevels.Damage;

public sealed class CEZLevelDamageSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;

    public float BaseFallingDamage { get; private set; }
    public float BaseFallingOtherDamage { get; private set; }
    public float BaseFallingStunTime { get; private set; }
    public float BaseFallingOtherStunTime { get; private set; }

    private static readonly ProtoId<DamageTypePrototype> BluntDamageType = "Blunt";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysicsComponent, CEZLevelHitEvent>(OnFallDamage);

        _config.OnValueChanged(CCVars.CEBaseFallingDamage, i => BaseFallingDamage = i, true);
        _config.OnValueChanged(CCVars.CEBaseFallingOtherDamage, i => BaseFallingOtherDamage = i, true);
        _config.OnValueChanged(CCVars.CEBaseFallingStunTime, i => BaseFallingStunTime = i, true);
        _config.OnValueChanged(CCVars.CEBaseFallingOtherStunTime, i => BaseFallingOtherStunTime = i, true);
    }

    private void OnFallDamage(Entity<PhysicsComponent> ent, ref CEZLevelHitEvent args)
    {
        var damageModifier = 1f;
        var stunModifier = 1f;

        List<EntityUid> redDamageFlash = new();

        var damageToOtherEv = new CEZFallingOnTargetDamageCalculateEvent(args.ImpactPower);
        RaiseLocalEvent(ent, damageToOtherEv);
        var otherDamage = damageToOtherEv.DamageMultiplier * BaseFallingOtherDamage * args.ImpactPower * ent.Comp.Mass;
        var otherStun = damageToOtherEv.StunMultiplier * BaseFallingOtherStunTime * args.ImpactPower * ent.Comp.Mass;

        // Calculate damage modifiers for the falling entity
        var damageToSelfEv = new CEZFallingDamageCalculateEvent(ent, args.ImpactPower);
        RaiseLocalEvent(ent, damageToSelfEv);
        damageModifier *= damageToSelfEv.DamageMultiplier;
        stunModifier *= damageToSelfEv.StunMultiplier;

        var entitiesAround = _lookup.GetEntitiesInRange(ent, 0.25f, LookupFlags.Uncontained);
        entitiesAround.Remove(ent); //Don't count self

        //Process entities we fell into
        var imFallOnEv = new CEZImFallOnEvent(entitiesAround, args.ImpactPower);
        RaiseLocalEvent(ent, imFallOnEv);

        foreach (var victim in entitiesAround)
        {
            // Calculate damage modifiers from entities being fallen upon
            var editDamageToSelfEv = new CEZFallingDamageCalculateEvent(ent, args.ImpactPower);
            RaiseLocalEvent(victim, editDamageToSelfEv);
            damageModifier *= editDamageToSelfEv.DamageMultiplier;
            stunModifier *= editDamageToSelfEv.StunMultiplier;

            var fellOnMeEv = new CEZFellOnMeEvent(ent, args.ImpactPower);
            RaiseLocalEvent(victim, fellOnMeEv);

            // Apply damage and stun to entities that were fallen upon
            if (otherStun > 0)
                _stun.TryKnockdown(victim, TimeSpan.FromSeconds(otherStun), true);
            if (otherDamage > 0)
            {
                if (_damage.TryChangeDamage(victim, new DamageSpecifier(_proto.Index(BluntDamageType), otherDamage)) != null && _net.IsClient)
                    redDamageFlash.Add(victim);
            }
        }

        var damageAmount = args.ImpactPower * args.ImpactPower * BaseFallingDamage * damageModifier;
        if (damageAmount > 0)
        {
            if (_damage.TryChangeDamage(ent.Owner, new DamageSpecifier(_proto.Index(BluntDamageType), damageAmount)) != null && _net.IsClient)
                redDamageFlash.Add(ent.Owner);
        }

        _color.RaiseEffect(Color.Red, redDamageFlash, Filter.Pvs(ent, entityManager: EntityManager));

        var knockdownTime = MathF.Min(args.ImpactPower * args.ImpactPower * BaseFallingStunTime * stunModifier, 5f);
        if (knockdownTime > 0)
            _stun.TryKnockdown(ent.Owner, TimeSpan.FromSeconds(knockdownTime), true);
    }
}

/// <summary>
/// This event is triggered both on the entity that fell and on all entities that it fell on.
/// Together, they calculate the damage and the duration that should be applied to the fallen entity.
/// </summary>
public sealed class CEZFallingDamageCalculateEvent(EntityUid fallen, float speed) : EntityEventArgs
{
    public EntityUid Fallen = fallen;

    public float DamageMultiplier = 1;
    public float StunMultiplier = 1;
    public float Speed = speed;
}

/// <summary>
/// Called on a falling entity to calculate how much damage it should inflict on everything it falls on.
/// </summary>
public sealed class CEZFallingOnTargetDamageCalculateEvent(float speed) : EntityEventArgs
{
    public float DamageMultiplier = 1;
    public float StunMultiplier = 1;
    public float Speed = speed;
}

/// <summary>
/// Event raised on a falling entity to inform it about the entities it is landing on and the impact speed.
/// </summary>
public sealed class CEZImFallOnEvent(HashSet<EntityUid> targets, float speed) : EntityEventArgs
{
    public HashSet<EntityUid> Targets = targets;
    public float Speed = speed;
}

/// <summary>
/// Event raised on an entity that is being fallen on to inform it about the falling entity and the impact speed.
/// </summary>
public sealed class CEZFellOnMeEvent(EntityUid fallen, float speed) : EntityEventArgs
{
    public EntityUid Fallen = fallen;
    public float Speed = speed;
}
