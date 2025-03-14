using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Systems;
using Content.Server.Construction;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Stack;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.FixedPoint;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using Content.Server.Construction.Completions;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;

namespace Content.Server.Destructible
{
    [UsedImplicitly]
    public sealed class DestructibleSystem : SharedDestructibleSystem
    {
        [Dependency] public readonly IRobustRandom Random = default!;
        public new IEntityManager EntityManager => base.EntityManager;

        [Dependency] public readonly AtmosphereSystem AtmosphereSystem = default!;
        [Dependency] public readonly AudioSystem AudioSystem = default!;
        [Dependency] public readonly BodySystem BodySystem = default!;
        [Dependency] public readonly ConstructionSystem ConstructionSystem = default!;
        [Dependency] public readonly ExplosionSystem ExplosionSystem = default!;
        [Dependency] public readonly StackSystem StackSystem = default!;
        [Dependency] public readonly TriggerSystem TriggerSystem = default!;
        [Dependency] public readonly SharedSolutionContainerSystem SolutionContainerSystem = default!;
        [Dependency] public readonly PuddleSystem PuddleSystem = default!;
        [Dependency] public readonly SharedContainerSystem ContainerSystem = default!;
        [Dependency] public readonly IPrototypeManager PrototypeManager = default!;
        [Dependency] public readonly IComponentFactory ComponentFactory = default!;
        [Dependency] public readonly IAdminLogManager AdminLogger = default!;
        [Dependency] private readonly ThrowingSystem _throwing = default!;
        [Dependency] private readonly SharedProjectileSystem _projectile = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<DestructibleComponent, DamageChangedEvent>(Execute);
            SubscribeLocalEvent<DestructibleComponent, DestructionEventArgs>(OnDestroyed);
            SubscribeLocalEvent<DestructibleComponent, ConstructionBeforeDeleteEvent>(OnDeconstruct);
        }

        /// <summary>
        ///     Check if any thresholds were reached. if they were, execute them.
        /// </summary>
        public void Execute(EntityUid uid, DestructibleComponent component, DamageChangedEvent args)
        {
            foreach (var threshold in component.Thresholds)
            {
                if (threshold.Reached(args.Damageable, this))
                {
                    RaiseLocalEvent(uid, new DamageThresholdReached(component, threshold), true);

                    // Convert behaviors into string for logs
                    var triggeredBehaviors = string.Join(", ", threshold.Behaviors.Select(b =>
                    {
                        if (b is DoActsBehavior doActsBehavior)
                        {
                            return $"{b.GetType().Name}:{doActsBehavior.Acts.ToString()}";
                        }
                        return b.GetType().Name;
                    }));

                    if (args.Origin != null)
                    {
                        AdminLogger.Add(LogType.Damaged, LogImpact.Medium,
                            $"{ToPrettyString(args.Origin.Value):actor} caused {ToPrettyString(uid):subject} to trigger [{triggeredBehaviors}]");
                    }
                    else
                    {
                        AdminLogger.Add(LogType.Damaged, LogImpact.Medium,
                            $"Unknown damage source caused {ToPrettyString(uid):subject} to trigger [{triggeredBehaviors}]");
                    }

                    threshold.Execute(uid, this, EntityManager, args.Origin);
                }

                // if destruction behavior (or some other deletion effect) occurred, don't run other triggers.
                if (EntityManager.IsQueuedForDeletion(uid) || Deleted(uid))
                    return;
            }
        }

        public void OnDestroyed(EntityUid uid, DestructibleComponent component, DestructionEventArgs args) => RemoveEmbedChildren(uid);

        public void OnDeconstruct(EntityUid uid, DestructibleComponent component, ConstructionBeforeDeleteEvent args) => RemoveEmbedChildren(uid);

        public void RemoveEmbedChildren(EntityUid uid)
        {
            var children = Transform(uid).ChildEnumerator;
            while (children.MoveNext(out var child))
            {
                if (!TryComp(child, out EmbeddableProjectileComponent? embed))
                    continue;

                _projectile.RemoveEmbed(child, embed);
                _throwing.TryThrow(child, Random.NextVector2(), 1f, friction: 100f); // very short distance
            }
        }

        // FFS this shouldn't be this hard. Maybe this should just be a field of the destructible component. Its not
        // like there is currently any entity that is NOT just destroyed upon reaching a total-damage value.
        /// <summary>
        ///     Figure out how much damage an entity needs to have in order to be destroyed.
        /// </summary>
        /// <remarks>
        ///     This assumes that this entity has some sort of destruction or breakage behavior triggered by a
        ///     total-damage threshold.
        /// </remarks>
        public FixedPoint2 DestroyedAt(EntityUid uid, DestructibleComponent? destructible = null)
        {
            if (!Resolve(uid, ref destructible, logMissing: false))
                return FixedPoint2.MaxValue;

            // We have nested for loops here, but the vast majority of components only have one threshold with 1-3 behaviors.
            // Really, this should probably just be a property of the damageable component.
            var damageNeeded = FixedPoint2.MaxValue;
            foreach (var threshold in destructible.Thresholds)
            {
                if (threshold.Trigger is not DamageTrigger trigger)
                    continue;

                foreach (var behavior in threshold.Behaviors)
                {
                    if (behavior is DoActsBehavior actBehavior &&
                        actBehavior.HasAct(ThresholdActs.Destruction | ThresholdActs.Breakage))
                    {
                        damageNeeded = Math.Min(damageNeeded.Float(), trigger.Damage);
                    }
                }
            }
            return damageNeeded;
        }
    }

    // Currently only used for destructible integration tests. Unless other uses are found for this, maybe this should just be removed and the tests redone.
    /// <summary>
    ///     Event raised when a <see cref="DamageThreshold"/> is reached.
    /// </summary>
    public sealed class DamageThresholdReached : EntityEventArgs
    {
        public readonly DestructibleComponent Parent;

        public readonly DamageThreshold Threshold;

        public DamageThresholdReached(DestructibleComponent parent, DamageThreshold threshold)
        {
            Parent = parent;
            Threshold = threshold;
        }
    }
}
