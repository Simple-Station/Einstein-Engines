using System.Linq;
using Content.Server.Flash.Components;
using Content.Shared.Flash.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.Flashbang;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Flash;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Examine;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using InventoryComponent = Content.Shared.Inventory.InventoryComponent;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Eye.Blinding.Systems;

namespace Content.Server.Flash
{
    internal sealed class FlashSystem : SharedFlashSystem
    {
        [Dependency] private readonly AppearanceSystem _appearance = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        [Dependency] private readonly SharedChargesSystem _charges = default!;
        [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly ExamineSystemShared _examine = default!;
        [Dependency] private readonly InventorySystem _inventory = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly StunSystem _stun = default!;
        [Dependency] private readonly TagSystem _tag = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
        [Dependency] private readonly BlindableSystem _blindingSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<FlashComponent, MeleeHitEvent>(OnFlashMeleeHit);
            // ran before toggling light for extra-bright lantern
            SubscribeLocalEvent<FlashComponent, UseInHandEvent>(OnFlashUseInHand, before: new []{ typeof(HandheldLightSystem) });
            SubscribeLocalEvent<FlashComponent, ThrowDoHitEvent>(OnFlashThrowHitEvent);
            SubscribeLocalEvent<InventoryComponent, FlashAttemptEvent>(OnInventoryFlashAttempt);
            SubscribeLocalEvent<FlashImmunityComponent, FlashAttemptEvent>(OnFlashImmunityFlashAttempt);
            SubscribeLocalEvent<PermanentBlindnessComponent, FlashAttemptEvent>(OnPermanentBlindnessFlashAttempt);
            SubscribeLocalEvent<TemporaryBlindnessComponent, FlashAttemptEvent>(OnTemporaryBlindnessFlashAttempt);
            SubscribeLocalEvent<BlindableComponent, FlashAttemptEvent>(OnBlindableFlashAttempt);
            SubscribeLocalEvent<EyeDamageOnFlashingComponent, FlashAttemptEvent>(OnEyeDamageOnFlashingFlashAttempt);
        }

        private void OnFlashMeleeHit(EntityUid uid, FlashComponent comp, MeleeHitEvent args)
        {
            if (!args.IsHit || !args.HitEntities.Any() || !UseFlash(uid, comp))
                return;

            args.Handled = true;
            foreach (var e in args.HitEntities)
            {
                Flash(e, args.User, uid, comp.FlashDuration, comp.SlowTo, melee: true, stunDuration: comp.MeleeStunDuration);
            }
        }

        private void OnFlashUseInHand(EntityUid uid, FlashComponent comp, UseInHandEvent args)
        {
            if (args.Handled || !UseFlash(uid, comp))
                return;

            args.Handled = true;
            FlashArea(uid, args.User, comp.Range, comp.AoeFlashDuration, comp.SlowTo, true, comp.Probability);
        }

        private void OnFlashThrowHitEvent(EntityUid uid, FlashComponent comp, ThrowDoHitEvent args)
        {
            if (!UseFlash(uid, comp))
                return;

            FlashArea(uid, args.User, comp.Range, comp.AoeFlashDuration, comp.SlowTo, false, comp.Probability);
        }

        private bool UseFlash(EntityUid uid, FlashComponent comp)
        {
            if (comp.Flashing)
                return false;

            TryComp<LimitedChargesComponent>(uid, out var charges);
            if (_charges.IsEmpty(uid, charges))
                return false;

            _charges.UseCharge(uid, charges);
            _audio.PlayPvs(comp.Sound, uid);
            comp.Flashing = true;
            _appearance.SetData(uid, FlashVisuals.Flashing, true);

            if (_charges.IsEmpty(uid, charges))
            {
                _appearance.SetData(uid, FlashVisuals.Burnt, true);
                _tag.AddTag(uid, "Trash");
                _popup.PopupEntity(Loc.GetString("flash-component-becomes-empty"), uid);
            }

            uid.SpawnTimer(400, () =>
            {
                _appearance.SetData(uid, FlashVisuals.Flashing, false);
                comp.Flashing = false;
            });

            return true;
        }

        public void Flash(EntityUid target,
            EntityUid? user,
            EntityUid? used,
            float flashDuration,
            float slowTo,
            bool displayPopup = true,
            bool melee = false,
            TimeSpan? stunDuration = null)
        {
            var attempt = new FlashAttemptEvent(target, user, used);
            RaiseLocalEvent(target, ref attempt, true);

            if (attempt.Cancelled)
                return;

            // don't paralyze, slowdown or convert to rev if the target is immune to flashes
            if (!_statusEffectsSystem.TryAddStatusEffect<FlashedComponent>(target, FlashedKey, TimeSpan.FromSeconds(flashDuration / 1000f), true))
                return;

            if (attempt.EyeDamage > 0)
                _blindingSystem.AdjustEyeDamage((target, null), attempt.EyeDamage);

            if (stunDuration != null)
            {
                _stun.TryKnockdown(target, stunDuration.Value, true);
            }
            else
            {
                _stun.TrySlowdown(target, TimeSpan.FromSeconds(flashDuration / 1000f), true,
                slowTo, slowTo);
            }

            if (displayPopup && user != null && target != user && Exists(user.Value))
            {
                _popup.PopupEntity(Loc.GetString("flash-component-user-blinds-you",
                    ("user", Identity.Entity(user.Value, EntityManager))), target, target);
            }
        }

        public override void FlashArea(Entity<FlashComponent?> source, EntityUid? user, float range, float duration, float slowTo = 0.8f, bool displayPopup = false, float probability = 1f, SoundSpecifier? sound = null)
        {
            var transform = Transform(source);
            var mapPosition = _transform.GetMapCoordinates(transform);
            var statusEffectsQuery = GetEntityQuery<StatusEffectsComponent>();
            var damagedByFlashingQuery = GetEntityQuery<DamagedByFlashingComponent>();

            foreach (var entity in _entityLookup.GetEntitiesInRange(transform.Coordinates, range))
            {
                if (!_random.Prob(probability))
                    continue;

                // Is the entity affected by the flash either through status effects or by taking damage?
                if (!statusEffectsQuery.HasComponent(entity) && !damagedByFlashingQuery.HasComponent(entity))
                    continue;

                // Check for entites in view
                // put damagedByFlashingComponent in the predicate because shadow anomalies block vision.
                if (!_examine.InRangeUnOccluded(entity, mapPosition, range, predicate: (e) => damagedByFlashingQuery.HasComponent(e)))
                    continue;

                // They shouldn't have flash removed in between right?
                Flash(entity, user, source, duration, slowTo, displayPopup);

                var distance = (mapPosition.Position - _transform.GetMapCoordinates(entity).Position).Length();
                RaiseLocalEvent(source, new AreaFlashEvent(range, distance, entity));
            }

            _audio.PlayPvs(sound, source, AudioParams.Default.WithVolume(1f).WithMaxDistance(3f));
        }

        private void OnInventoryFlashAttempt(EntityUid uid, InventoryComponent component, FlashAttemptEvent args)
        {
            foreach (var slot in new[] { "head", "eyes", "mask" })
            {
                if (args.Cancelled)
                    break;
                if (_inventory.TryGetSlotEntity(uid, slot, out var item, component))
                    RaiseLocalEvent(item.Value, ref args, true);
            }
        }

        private void OnFlashImmunityFlashAttempt(EntityUid uid, FlashImmunityComponent component, FlashAttemptEvent args)
        {
            if (component.Enabled)
                args.Cancel();
        }

        private void OnPermanentBlindnessFlashAttempt(EntityUid uid, PermanentBlindnessComponent component, FlashAttemptEvent args)
        {
            // check for total blindness
            if (component.Blindness == 0)
                args.Cancel();
        }

        private void OnTemporaryBlindnessFlashAttempt(EntityUid uid, TemporaryBlindnessComponent component, FlashAttemptEvent args)
        {
            args.Cancel();
        }

        private void OnBlindableFlashAttempt(EntityUid uid, BlindableComponent component, FlashAttemptEvent args)
        {
            if (component.IsBlind)
                args.Cancel();
        }

        private void OnEyeDamageOnFlashingFlashAttempt(EntityUid uid, EyeDamageOnFlashingComponent component, ref FlashAttemptEvent args)
        {
            args.DurationMultiplier = component.FlashDurationMultiplier;

            if (_random.Prob(component.EyeDamageChance))
                args.EyeDamage = component.EyeDamage;
        }
    }

    /// <summary>
    ///     Called before a flash is used to check if the attempt is cancelled by blindness, items or FlashImmunityComponent.
    ///     Raised on the target hit by the flash, the user of the flash and the flash used.
    /// </summary>
    [ByRefEvent]
    public sealed class FlashAttemptEvent : CancellableEntityEventArgs
    {
        public readonly EntityUid Target;
        public readonly EntityUid? User;
        public readonly EntityUid? Used;

        [DataField]
        public float DurationMultiplier = 1f;

        [DataField]
        public int EyeDamage;

        public FlashAttemptEvent(EntityUid target, EntityUid? user, EntityUid? used)
        {
            Target = target;
            User = user;
            Used = used;
        }
    }
}
