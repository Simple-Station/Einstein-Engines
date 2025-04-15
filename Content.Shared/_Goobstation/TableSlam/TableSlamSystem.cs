using System.Linq;
using Content.Shared.Contests;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions.Events;
using Content.Shared.Climbing.Components;
using Content.Shared.CombatMode;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Climbing.Systems;

namespace Content.Shared._Goobstation.TableSlam;

/// <summary>
/// This handles...
/// </summary>
public sealed class TableSlamSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ContestsSystem _contestsSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ClimbSystem _climbing = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PullerComponent, MeleeHitEvent>(OnMeleeHit);
        //SubscribeLocalEvent<PullableComponent, StartCollideEvent>(OnStartCollide);
//      SubscribeLocalEvent<KnockedDownComponent, DisarmAttemptEvent>(OnDisarmAttemptEvent);
    }

// Deprecated, this feature has been moved to SharedStunSystem.cs
 /* private void OnDisarmAttemptEvent(Entity<KnockedDownComponent> ent, ref DisarmAttemptEvent args)
    {
        if (!TryComp<ClimbingComponent>(ent, out var component) || !component.IsClimbing || HasComp<StunnedComponent>(ent))
            return;
        _stunSystem.TryParalyze(ent, TimeSpan.FromSeconds(3), false);

    }*/

    /*public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var tabledQuery = EntityQueryEnumerator<PostTabledComponent>();
        while (tabledQuery.MoveNext(out var uid, out var comp))
        {
            if (_gameTiming.CurTime >= comp.PostTabledShovableTime)
             RemComp<PostTabledComponent>(uid);
        }
    }*/

    private void OnMeleeHit(Entity<PullerComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.GrabStage < GrabStage.Suffocate
            || ent.Comp.Pulling == null || (args.HitEntities.Count is > 1 or 0))
            return;

        if(!TryComp<PullableComponent>(ent.Comp.Pulling, out var pullableComponent))
            return;

        var target = args.HitEntities.ElementAt(0);
        if (!HasComp<BonkableComponent>(target)) // checks if its a table.
            return;

        var massContest = _contestsSystem.MassContest(ent, ent.Comp.Pulling.Value);
        var attemptChance = Math.Clamp(1 * massContest, 0, 1);
        var attemptRoundedToNearestQuarter = Math.Round(attemptChance * 4, MidpointRounding.ToEven) / 4;
        if(_random.Prob((float) attemptRoundedToNearestQuarter)) // base chance to table slam someone is 1 if your mass ratio is less than 1 then you're going to have a harder time slamming somebody.
            TryTableSlam((ent.Comp.Pulling.Value, pullableComponent), ent, target);
    }

    public void TryTableSlam(Entity<PullableComponent> ent, Entity<PullerComponent> pullerEnt, EntityUid tableUid)
    {
        if (!_transformSystem.InRange(ent.Owner.ToCoordinates(), tableUid.ToCoordinates(), 2f)
            || !TryComp<ClimbableComponent>(tableUid, out var component)
            || !HasComp<BonkableComponent>(tableUid))
            return;

        _stunSystem.TryKnockdown(ent, TimeSpan.FromSeconds(3), false);
        _climbing.TryClimb(pullerEnt, ent, tableUid, out _, component, null, true);
        _pullingSystem.TryStopPull(ent, ent.Comp, pullerEnt, ignoreGrab: true);
        if (TryComp<GlassTableComponent>(tableUid, out var glassTableComponent))
        {
            _damageableSystem.TryChangeDamage(tableUid, glassTableComponent.TableDamage, origin: ent, targetPart: TargetBodyPart.Torso);
            _damageableSystem.TryChangeDamage(tableUid, glassTableComponent.ClimberDamage, origin: ent);
            _stunSystem.TryParalyze(ent, TimeSpan.FromSeconds(3), false);
        }
        else
        {
            _damageableSystem.TryChangeDamage(ent,
                new DamageSpecifier()
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", ent.Comp.TabledDamage } },
                },
                targetPart: TargetBodyPart.Torso);
            _damageableSystem.TryChangeDamage(ent,
                new DamageSpecifier()
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", ent.Comp.TabledDamage } },
                });
        }
        _staminaSystem.TakeStaminaDamage(ent, ent.Comp.TabledStaminaDamage);
        //_audioSystem.PlayPvs("/Audio/Effects/thudswoosh.ogg", uid);
    }

   /* private void OnStartCollide(Entity<PullableComponent> ent, ref StartCollideEvent args)
    {
        if (!HasComp<BonkableComponent>(args.OtherEntity))
            return;

        var modifierOnGlassBreak = 1;
        if (TryComp<GlassTableComponent>(args.OtherEntity, out var glassTableComponent))
        {
            _damageableSystem.TryChangeDamage(args.OtherEntity, glassTableComponent.TableDamage, origin: ent, targetPart: TargetBodyPart.Torso);
            _damageableSystem.TryChangeDamage(args.OtherEntity, glassTableComponent.ClimberDamage, origin: ent);
            modifierOnGlassBreak = 2;
        }
        else
        {
            _damageableSystem.TryChangeDamage(ent,
                new DamageSpecifier()
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", ent.Comp.TabledDamage } },
                },
                targetPart: TargetBodyPart.Torso);
            _damageableSystem.TryChangeDamage(ent,
                new DamageSpecifier()
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", ent.Comp.TabledDamage } },
                });
        }
        _staminaSystem.TakeStaminaDamage(ent, ent.Comp.TabledStaminaDamage);
        _stunSystem.TryKnockdown(ent, TimeSpan.FromSeconds(3 * modifierOnGlassBreak), false);

        //_audioSystem.PlayPvs("/Audio/Effects/thudswoosh.ogg", uid);
    }*/
}
