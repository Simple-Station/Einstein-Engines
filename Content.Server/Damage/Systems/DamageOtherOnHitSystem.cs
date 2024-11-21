using Content.Server.Administration.Logs;
using Content.Server.Damage.Components;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Damage.Systems
{
    public sealed class DamageOtherOnHitSystem : SharedDamageOtherOnHitSystem
    {
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
        [Dependency] private readonly SharedCameraRecoilSystem _sharedCameraRecoil = default!;
        [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
        [Dependency] private readonly ThrownItemSystem _thrownItem = default!;
        [Dependency] private readonly PhysicsSystem _physics = default!;
        [Dependency] private readonly MeleeSoundSystem _meleeSound = default!;
        [Dependency] private readonly StaminaSystem _stamina = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly IPrototypeManager _protoManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrowDoHitEvent>(OnDoHit);
            SubscribeLocalEvent<StaminaComponent, ThrowAttemptEvent>(OnThrowAttempt);
            SubscribeLocalEvent<StaminaComponent, BeforeThrowEvent>(OnBeforeThrow);
            SubscribeLocalEvent<DamageOtherOnHitComponent, ThrownEvent>(OnThrown);
            SubscribeLocalEvent<DamageOtherOnHitComponent, DamageExamineEvent>(OnDamageExamine);
        }

        private void OnDoHit(EntityUid uid, DamageOtherOnHitComponent component, ThrowDoHitEvent args)
        {
            if (component.HitQuantity >= component.MaxHitQuantity)
                return;

            var damage = component.Damage;
            var soundHit = component.SoundHit;
            var soundNoDamage = component.SoundNoDamage;

            if (component.InheritMeleeStats && TryComp<MeleeWeaponComponent>(uid, out var melee))
            {
                if (damage.Empty)
                    damage = melee.Damage;
                if (soundHit == null)
                    soundHit = melee.SoundHit;
                if (soundNoDamage == null)
                    soundNoDamage = melee.SoundNoDamage;
            }

            var modifiedDamage = _damageable.TryChangeDamage(args.Target, damage, component.IgnoreResistances, origin: args.Component.Thrower);

            // Log damage only for mobs. Useful for when people throw spears at each other, but also avoids log-spam when explosions send glass shards flying.
            if (modifiedDamage != null)
            {
                if (HasComp<MobStateComponent>(args.Target))
                    _adminLogger.Add(LogType.ThrowHit, $"{ToPrettyString(args.Target):target} received {modifiedDamage.GetTotal():damage} damage from collision");

                _meleeSound.PlayHitSound(args.Target, null, SharedMeleeWeaponSystem.GetHighestDamageSound(modifiedDamage, _protoManager), null,
                    soundHit, soundNoDamage);
            }

            if (modifiedDamage is { Empty: false })
            {
                _color.RaiseEffect(Color.Red, new List<EntityUid>() { args.Target }, Filter.Pvs(args.Target, entityManager: EntityManager));
            }

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

        private void OnThrowAttempt(EntityUid uid, StaminaComponent component, ref ThrowAttemptEvent args)
        {
            if (TryComp<DamageOtherOnHitComponent>(args.ItemUid, out var damage) &&
                component.CritThreshold - component.StaminaDamage <= damage.StaminaCost)
            {
                args.Cancel();
                _popup.PopupEntity(Loc.GetString("throw-no-stamina", ("item", args.ItemUid)), uid, uid);
            }
        }

        private void OnBeforeThrow(EntityUid uid, StaminaComponent component, BeforeThrowEvent args)
        {
            if (TryComp<DamageOtherOnHitComponent>(args.ItemUid, out var damage))
                _stamina.TakeStaminaDamage(uid, damage.StaminaCost, component, visual: false);
        }

        private void OnThrown(EntityUid uid, DamageOtherOnHitComponent component, ThrownEvent args)
        {
            component.HitQuantity = 0;
        }

        private void OnDamageExamine(EntityUid uid, DamageOtherOnHitComponent component, ref DamageExamineEvent args)
        {
            var damage = component.Damage;
            if (component.InheritMeleeStats &&
                damage.Empty &&
                TryComp<MeleeWeaponComponent>(uid, out var melee))
                damage = melee.Damage;

            _damageExamine.AddDamageExamine(args.Message, damage, Loc.GetString("damage-throw"));

            if (component.StaminaCost == 0)
                return;

            var staminaCostMarkup = FormattedMessage.FromMarkupOrThrow(
                Loc.GetString("damage-stamina-cost",
                ("type", Loc.GetString("damage-throw")), ("cost", component.StaminaCost)));
            args.Message.PushNewline();
            args.Message.AddMessage(staminaCostMarkup);
        }
    }
}
