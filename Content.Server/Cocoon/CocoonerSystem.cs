using Content.Shared.Cocoon;
using Content.Shared.IdentityManagement;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;
using Content.Shared.Stunnable;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Server.Popups;
using Content.Server.DoAfter;
using Content.Server.Speech.Components;
using Robust.Shared.Containers;
using Content.Shared.Mobs.Components;

namespace Content.Server.Cocoon
{
    public sealed class CocooningSystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
        [Dependency] private readonly BlindableSystem _blindableSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

        private const string BodySlot = "body_slot";

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<CocoonerComponent, GetVerbsEvent<InnateVerb>>(AddCocoonVerb);
            SubscribeLocalEvent<CocoonComponent, EntInsertedIntoContainerMessage>(OnCocEntInserted);
            SubscribeLocalEvent<CocoonComponent, EntRemovedFromContainerMessage>(OnCocEntRemoved);
            SubscribeLocalEvent<CocoonComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<CocoonerComponent, CocoonDoAfterEvent>(OnCocoonDoAfter);
        }

        private void AddCocoonVerb(EntityUid uid, CocoonerComponent component, GetVerbsEvent<InnateVerb> args)
        {
            if (!args.CanAccess || !args.CanInteract || !HasComp<MobStateComponent>(args.Target))
                return;

            InnateVerb verb = new()
            {
                Act = () =>
                {
                    StartCocooning(uid, component, args.Target);
                },
                Text = Loc.GetString("cocoon"),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void OnCocEntInserted(EntityUid uid, CocoonComponent component, EntInsertedIntoContainerMessage args)
        {
            component.Victim = args.Entity;

            if (TryComp<ReplacementAccentComponent>(args.Entity, out var currentAccent))
                component.OldAccent = currentAccent.Accent;

            EnsureComp<ReplacementAccentComponent>(args.Entity).Accent = "mumble";
            EnsureComp<StunnedComponent>(args.Entity);

            _blindableSystem.UpdateIsBlind(args.Entity);
        }

        private void OnCocEntRemoved(EntityUid uid, CocoonComponent component, EntRemovedFromContainerMessage args)
        {
            if (TryComp<ReplacementAccentComponent>(args.Entity, out var replacement))
                replacement.Accent = component.OldAccent ?? replacement.Accent;
            else
                RemComp<ReplacementAccentComponent>(args.Entity);

            RemComp<StunnedComponent>(args.Entity);
            _blindableSystem.UpdateIsBlind(args.Entity);
        }

        private void OnDamageChanged(EntityUid uid, CocoonComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased || args.DamageDelta == null || component.Victim == null)
                return;

            var damage = args.DamageDelta * component.DamagePassthrough;
            _damageableSystem.TryChangeDamage(component.Victim, damage);
        }

        private void StartCocooning(EntityUid uid, CocoonerComponent component, EntityUid target)
        {
            _popupSystem.PopupEntity(Loc.GetString("cocoon-start-third-person", ("target", Identity.Entity(target, EntityManager)), ("spider", Identity.Entity(uid, EntityManager))), uid,
                Shared.Popups.PopupType.MediumCaution);

            var delay = component.CocoonDelay;

            if (HasComp<KnockedDownComponent>(target))
                delay *= component.CocoonKnockdownMultiplier;

            var args = new DoAfterArgs(EntityManager, uid, delay, new CocoonDoAfterEvent(), uid, target: target)
            {
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
            };

            _doAfter.TryStartDoAfter(args);
        }

        private void OnCocoonDoAfter(EntityUid uid, CocoonerComponent component, CocoonDoAfterEvent args)
        {
            if (args.Handled || args.Cancelled || args.Args.Target == null)
                return;

            var spawnProto = HasComp<HumanoidAppearanceComponent>(args.Args.Target) ? "CocoonedHumanoid" : "CocoonSmall";
            Transform(args.Args.Target.Value).AttachToGridOrMap();
            var cocoon = Spawn(spawnProto, Transform(args.Args.Target.Value).Coordinates);

            if (!TryComp<ItemSlotsComponent>(cocoon, out var slots))
                return;

            _itemSlots.SetLock(cocoon, BodySlot, false, slots);
            _itemSlots.TryInsert(cocoon, BodySlot, args.Args.Target.Value, args.Args.User);
            _itemSlots.SetLock(cocoon, BodySlot, true, slots);

            var impact = (spawnProto == "CocoonedHumanoid") ? LogImpact.High : LogImpact.Medium;

            _adminLogger.Add(LogType.Action, impact, $"{ToPrettyString(args.Args.User):player} cocooned {ToPrettyString(args.Args.Target.Value):target}");
            args.Handled = true;
        }
    }
}
