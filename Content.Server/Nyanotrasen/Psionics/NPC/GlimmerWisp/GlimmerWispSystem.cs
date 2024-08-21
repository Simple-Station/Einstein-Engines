using Content.Server.DoAfter;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Damage;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Verbs;
using Content.Shared.Psionics.Events;
using Content.Shared.Rejuvenate;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Pulling.Components;
using Content.Server.Popups;
using Content.Server.NPC.Systems;
using Content.Server.Carrying;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using Robust.Server.Audio;

namespace Content.Server.Psionics.NPC.GlimmerWisp
{
    public sealed class GlimmerWispSystem : EntitySystem
    {
        [Dependency] private readonly DamageableSystem _damageable = default!;
        [Dependency] private readonly DoAfterSystem _doAfter = default!;
        [Dependency] private readonly MobStateSystem _mobs = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
        [Dependency] private readonly PopupSystem _popups = default!;
        [Dependency] private readonly AudioSystem _audio = default!;
        [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<GlimmerWispComponent, GetVerbsEvent<InnateVerb>>(AddDrainVerb);
            SubscribeLocalEvent<GlimmerWispComponent, GlimmerWispDrainDoAfterEvent>(OnDrain);
        }

        private void AddDrainVerb(EntityUid uid, GlimmerWispComponent component, GetVerbsEvent<InnateVerb> args)
        {
            if (args.User == args.Target)
                return;
            if (!args.CanAccess)
                return;
            if (!HasComp<PsionicComponent>(args.Target))
                return;
            if (!_mobs.IsCritical(args.Target))
                return;

            InnateVerb verb = new()
            {
                Act = () =>
                {
                    StartLifeDrain(uid, args.Target, component);
                },
                Text = Loc.GetString("verb-life-drain"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Nyanotrasen/Icons/verbiconfangs.png")),
                Priority = 2
            };
            args.Verbs.Add(verb);
        }

        private void OnDrain(EntityUid uid, GlimmerWispComponent component, GlimmerWispDrainDoAfterEvent args)
        {
            component.IsDraining = false;

            if (args.Handled || args.Args.Target == null)
            {
                component.DrainAudioStream = _audio.Stop(component.DrainAudioStream);
                return;
            }

            if (args.Cancelled)
            {
                component.DrainAudioStream = _audio.Stop(component.DrainAudioStream);

                if (TryComp<PullableComponent>(args.Args.Target.Value, out var pullable) && pullable.Puller != null)
                    _npcFaction.AggroEntity(uid, pullable.Puller.Value);

                if (TryComp<BeingCarriedComponent>(args.Args.Target.Value, out var carried))
                    _npcFaction.AggroEntity(uid, carried.Carrier);

                return;
            }

            _popups.PopupEntity(Loc.GetString("life-drain-second-end", ("wisp", uid)), args.Args.Target.Value, args.Args.Target.Value, Shared.Popups.PopupType.LargeCaution);
            _popups.PopupEntity(Loc.GetString("life-drain-third-end", ("wisp", uid), ("target", args.Args.Target.Value)), args.Args.Target.Value, Filter.PvsExcept(args.Args.Target.Value), true, Shared.Popups.PopupType.LargeCaution);

            var rejEv = new RejuvenateEvent();
            RaiseLocalEvent(uid, rejEv);

            _audio.PlayPvs(component.DrainFinishSoundPath, uid);

            DamageSpecifier damage = new();
            damage.DamageDict.Add("Asphyxiation", 200);
            _damageable.TryChangeDamage(args.Args.Target.Value, damage, true, origin: uid);
        }


        public bool NPCStartLifedrain(EntityUid uid, EntityUid target, GlimmerWispComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return false;
            if (!HasComp<PsionicComponent>(target))
                return false;
            if (!_mobs.IsCritical(target))
                return false;
            if (!_actionBlocker.CanInteract(uid, target))
                return false;

            StartLifeDrain(uid, target, component);
            return true;
        }

        public void StartLifeDrain(EntityUid uid, EntityUid target, GlimmerWispComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.DrainTarget = target;
            _popups.PopupEntity(Loc.GetString("life-drain-second-start", ("wisp", uid)), target, target, Shared.Popups.PopupType.LargeCaution);
            _popups.PopupEntity(Loc.GetString("life-drain-third-start", ("wisp", uid), ("target", target)), target, Filter.PvsExcept(target), true, Shared.Popups.PopupType.LargeCaution);

            component.DrainAudioStream = _audio.PlayPvs(component.DrainSoundPath, target).Value.Entity;
            component.IsDraining = true;

            var ev = new GlimmerWispDrainDoAfterEvent();
            var args = new DoAfterArgs(EntityManager, uid, component.DrainDelay, ev, uid, target: target)
            {
                BreakOnTargetMove = true,
                BreakOnUserMove = false,
                DistanceThreshold = 2f,
                NeedHand = false
            };

            _doAfter.TryStartDoAfter(args);
        }
    }
}
