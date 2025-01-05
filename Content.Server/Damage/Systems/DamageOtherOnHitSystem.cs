using Content.Shared.Camera;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Effects;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Server.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Damage.Systems
{
    public sealed class DamageOtherOnHitSystem : SharedDamageOtherOnHitSystem
    {
        [Dependency] private readonly DamageExamineSystem _damageExamine = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly StaminaSystem _stamina = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<StaminaComponent, BeforeThrowEvent>(OnBeforeThrow, after: [typeof(PacificationSystem)]);
            SubscribeLocalEvent<DamageOtherOnHitComponent, DamageExamineEvent>(OnDamageExamine, after: [typeof(MeleeWeaponSystem)]);
        }

        private void OnBeforeThrow(EntityUid uid, StaminaComponent component, ref BeforeThrowEvent args)
        {
            if (args.Cancelled || !TryComp<DamageOtherOnHitComponent>(args.ItemUid, out var damage))
                return;

            if (component.CritThreshold - component.StaminaDamage <= damage.StaminaCost)
            {
                args.Cancelled = true;
                _popup.PopupEntity(Loc.GetString("throw-no-stamina", ("item", args.ItemUid)), uid, uid);
                return;
            }

            _stamina.TakeStaminaDamage(uid, damage.StaminaCost, component, visual: false);
        }

        private void OnDamageExamine(EntityUid uid, DamageOtherOnHitComponent component, ref DamageExamineEvent args)
        {
            _damageExamine.AddDamageExamine(args.Message, GetDamage(uid, component, args.User), Loc.GetString("damage-throw"));

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
