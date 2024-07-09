using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Psionics.Abilities;
using Content.Shared.Psionics.Glimmer;
using Content.Server.Atmos.Components;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Actions.Events;
using Content.Server.Explosion.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.Psionics;

namespace Content.Server.Psionics.Abilities
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedTransformSystem _xform = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly GunSystem _gunSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PyrokinesisPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<PyrokinesisPrechargeActionEvent>(OnPrecharge);
            SubscribeLocalEvent<PyrokinesisPowerComponent, PyrokinesisPrechargeDoAfterEvent>(OnDoAfter);
        }

        private void OnInit(EntityUid uid, PyrokinesisPowerComponent component, ComponentInit args)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);
            _actions.AddAction(uid, ref component.PyrokinesisPrechargeActionEntity, component.PyrokinesisPrechargeActionId);
            _actions.TryGetActionData(component.PyrokinesisPrechargeActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.PyrokinesisPrechargeActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));

            psionic.ActivePowers.Add(component);
            psionic.PsychicFeedback.Add(component.PyrokinesisFeedback);
            psionic.Amplification += 1f;
        }

        private void OnPrecharge(PyrokinesisPrechargeActionEvent args)
        {
            if (_psionics.CheckCanSelfCast(args.Performer, out var psionic)
                && TryComp<PyrokinesisPowerComponent>(args.Performer, out var pyroComp))
            {
                _actions.AddAction(args.Performer, ref pyroComp.PyrokinesisActionEntity, pyroComp.PyrokinesisActionId);
                _actions.TryGetActionData(pyroComp.PyrokinesisActionEntity, out var actionData);
                if (actionData is { UseDelay: not null })
                    _actions.SetCooldown(pyroComp.PyrokinesisActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));
                _actions.TryGetActionData(pyroComp.PyrokinesisPrechargeActionEntity, out var prechargeData);
                if (prechargeData is { UseDelay: not null })
                    _actions.StartUseDelay(pyroComp.PyrokinesisPrechargeActionEntity);

                if (_glimmerSystem.GlimmerOutput >= 100 * psionic.Dampening)
                {
                    _popup.PopupEntity(Loc.GetString(pyroComp.PyrokinesisObviousPopup, ("entity", args.Performer)), args.Performer, PopupType.Medium);
                    _audioSystem.PlayPvs(pyroComp.SoundUse, args.Performer);
                }
                else
                    _popup.PopupEntity(Loc.GetString(pyroComp.PyrokinesisSubtlePopup), args.Performer, args.Performer, PopupType.Medium);

                pyroComp.FireballThrown = false;

                var ev = new PyrokinesisPrechargeDoAfterEvent(_gameTiming.CurTime);
                var duration = TimeSpan.FromSeconds(pyroComp.ResetDuration.Seconds + psionic.Dampening);

                _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, args.Performer, duration, ev, args.Performer, args.Performer, args.Performer)
                {
                    BlockDuplicate = true,
                    Hidden = true,
                }, out var doAfterId);

                pyroComp.ResetDoAfter = doAfterId;
            }
        }

        private void OnShutdown(EntityUid uid, PyrokinesisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.PyrokinesisPrechargeActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.PyrokinesisFeedback);
                psionic.Amplification -= 1f;
            }
        }

        private void OnPowerUsed(PyrokinesisPowerActionEvent args)
        {
            if (_psionics.CheckCanSelfCast(args.Performer, out var psionic)
                && TryComp<PyrokinesisPowerComponent>(args.Performer, out var pyroComp))
            {
                var spawnCoords = Transform(args.Performer).Coordinates;

                var ent = Spawn("ProjectileAnomalyFireball", spawnCoords);

                if (_glimmerSystem.GlimmerOutput <= 25 * psionic.Dampening)
                    EnsureComp<PsionicallyInvisibleComponent>(ent);

                if (TryComp<ExplosiveComponent>(ent, out var fireball))
                {
                    var psionicFactor = psionic.Amplification * _glimmerSystem.GetGlimmerEquilibriumRatio();
                    fireball.MaxIntensity = 2 * psionicFactor;
                    fireball.IntensitySlope = 1 * psionicFactor;
                    fireball.TotalIntensity = 25 * psionicFactor;

                    if (_glimmerSystem.GlimmerOutput >= GlimmerSystem.GlimmerEquilibrium)
                        fireball.CanCreateVacuum = true;
                    else fireball.CanCreateVacuum = false;

                    if (psionic.Amplification > 5)
                        if (EnsureComp<IgniteOnCollideComponent>(ent, out var ignite))
                            ignite.FireStacks = 0.2f * psionic.Amplification;
                }

                var direction = args.Target.ToMapPos(EntityManager, _xform) - spawnCoords.ToMapPos(EntityManager, _xform);

                _gunSystem.ShootProjectile(ent, direction, new System.Numerics.Vector2(0, 0), args.Performer, args.Performer, 20f);
                _actions.RemoveAction(args.Performer, pyroComp.PyrokinesisActionEntity);
                _doAfterSystem.Cancel(pyroComp.ResetDoAfter);
                _psionics.LogPowerUsed(args.Performer, "pyrokinesis", psionic, 6, 8);
                pyroComp.FireballThrown = true;
                args.Handled = true;
            }
        }

        private void OnDoAfter(EntityUid uid, PyrokinesisPowerComponent component, PyrokinesisPrechargeDoAfterEvent args)
        {
            if (!args.Cancelled && TryComp<PsionicComponent>(uid, out var psionic))
            {
                if (!component.FireballThrown)
                {
                    _actions.TryGetActionData(component.PyrokinesisPrechargeActionEntity, out var actionData);
                    if (actionData is { UseDelay: not null })
                        _actions.SetCooldown(component.PyrokinesisPrechargeActionEntity, TimeSpan.FromSeconds(15 - psionic.Dampening));

                    _popup.PopupEntity(Loc.GetString(component.PyrokinesisRefundCooldown), uid, uid, PopupType.Medium);
                }
                _actions.RemoveAction(uid, component.PyrokinesisActionEntity);
            }
        }
    }
}
