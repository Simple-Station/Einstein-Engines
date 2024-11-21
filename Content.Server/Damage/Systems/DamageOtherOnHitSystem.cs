using Content.Server.Administration.Logs;
using Content.Server.Damage.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Damage.Systems
{
    public sealed class DamageOtherOnHitSystem : SharedDamageOtherOnHitSystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly GunSystem _guns = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
        [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
        [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
        [Dependency] private readonly ThrownItemSystem _thrownItem = default!;
        [Dependency] private readonly PhysicsSystem _physics = default!;
        [Dependency] private readonly MeleeSoundSystem _meleeSound = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowDoHitEvent>(OnDoHit);
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrownEvent>(OnThrown);
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowAttemptEvent>(OnThrowAttempt);
            SubscribeLocalEvent<DamageOtherOnHitComponent, DamageExamineEvent>(OnDamageExamine);
        }

        private void OnDoHit(EntityUid uid, DamageOtherOnHitComponent component, ThrowDoHitEvent args)
        {
            if (component.HitQuantity >= component.MaxHitQuantity)
                return;

            var dmg = _damageable.TryChangeDamage(args.Target, component.Damage, component.IgnoreResistances, origin: args.Component.Thrower);

            // Log damage only for mobs. Useful for when people throw spears at each other, but also avoids log-spam when explosions send glass shards flying.
            if (dmg != null && HasComp<MobStateComponent>(args.Target))
                _adminLogger.Add(LogType.ThrowHit, $"{ToPrettyString(args.Target):target} received {dmg.GetTotal():damage} damage from collision");

            if (dmg is { Empty: false })
            {
                _color.RaiseEffect(Color.Red, new List<EntityUid>() { args.Target }, Filter.Pvs(args.Target, entityManager: EntityManager));
            }

            _guns.PlayImpactSound(args.Target, dmg, null, false);
            if (TryComp<PhysicsComponent>(uid, out var body) && body.LinearVelocity.LengthSquared() > 0f)
            {
                var direction = body.LinearVelocity.Normalized();
                _sharedCameraRecoil.KickCamera(args.Target, direction);
            }

            // TODO: If more stuff touches this then handle it after.
            if (TryComp<PhysicsComponent>(uid, out var physics))
            {
                _thrownItem.LandComponent(args.Thrown, args.Component, physics, false);

                if (!HasComp<EmbeddableProjectileComponent>(args.Thrown))
                {
                    var newVelocity = physics.LinearVelocity;
                    newVelocity.X = -newVelocity.X / 4;
                    newVelocity.Y = -newVelocity.Y / 4;
                    _physics.SetLinearVelocity(uid, newVelocity, body: physics);
                }
            }

            component.HitQuantity += 1;
        }

        private void OnThrown(EntityUid uid, DamageOtherOnHitComponent component, ThrownEvent args)
        {
            component.HitQuantity = 0;
        }

        private void OnThrowAttempt(EntityUid ent, DamageOtherOnHitComponent component, ref ThrowAttemptEvent args)
        {
            if (TryComp<StaminaComponent>(args.Uid, out var stamina)
                && stamina.CritThreshold - stamina.StaminaDamage <= component.StaminaCost)
            {
                args.Cancel("throw-no-stamina");
            }
        }


        private void OnDamageExamine(EntityUid uid, DamageOtherOnHitComponent component, ref DamageExamineEvent args)
        {
            _damageExamine.AddDamageExamine(args.Message, component.Damage, Loc.GetString("damage-throw"));

            if (component.StaminaCost != 0)
            {
                var staminaCostMarkup = FormattedMessage.FromMarkupOrThrow(
                    Loc.GetString("damage-stamina-cost",
                    ("type", Loc.GetString("damage-throw")), ("cost", component.StaminaCost)));
                args.Message.PushNewline();
                args.Message.AddMessage(staminaCostMarkup);
            }
        }
    }
}
