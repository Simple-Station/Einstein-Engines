using Content.Shared.Actions;
using Content.Shared.Psionics.Abilities;
using Content.Shared.Psionics.Glimmer;
using Content.Server.Atmos.Components;
using Content.Server.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Content.Shared.Actions.Events;
using Content.Server.Explosion.Components;
using Robust.Shared.Map;

namespace Content.Server.Psionics.Abilities
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly TransformSystem _xform = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly GunSystem _gunSystem = default!;
        [Dependency] private readonly GlimmerSystem _glimmerSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PyrokinesisPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, PyrokinesisPowerComponent component, ComponentInit args)
        {
            _actions.AddAction(uid, ref component.PyrokinesisActionEntity, component.PyrokinesisActionId);
            _actions.TryGetActionData( component.PyrokinesisActionEntity, out var actionData);
            if (actionData is { UseDelay: not null })
                _actions.StartUseDelay(component.PyrokinesisActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Add(component);
                psionic.PsychicFeedback.Add(component.PyrokinesisFeedback);
                psionic.Amplification += 1f;
            }
        }

        private void OnShutdown(EntityUid uid, PyrokinesisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.PyrokinesisActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.PyrokinesisFeedback);
                psionic.Amplification -= 1f;
            }
        }

        private void OnPowerUsed(PyrokinesisPowerActionEvent args)
        {
            if (!HasComp<PsionicInsulationComponent>(args.Performer)
                && TryComp<PsionicComponent>(args.Performer, out var psionic))
            {
                var spawnCoords = Transform(args.Performer).Coordinates;

                var ent = Spawn("ProjectileAnomalyFireball", spawnCoords);

                if (TryComp<ExplosiveComponent>(ent, out var fireball))
                {
                    fireball.MaxIntensity = (int) MathF.Round(5 * psionic.Amplification);
                    fireball.IntensitySlope = (int) MathF.Round(1 * psionic.Amplification);
                    fireball.TotalIntensity = (int) MathF.Round(10 * psionic.Amplification);

                    if (_glimmerSystem.Glimmer >= 500)
                        fireball.CanCreateVacuum = true;
                    else fireball.CanCreateVacuum = false;

                    if (psionic.Amplification > 5)
                        if (EnsureComp<IgniteOnCollideComponent>(ent, out var ignite))
                            ignite.FireStacks = 0.2f * psionic.Amplification;
                }

                var direction = args.Target.ToMapPos(EntityManager, _xform) - spawnCoords.ToMapPos(EntityManager, _xform);

                _gunSystem.ShootProjectile(ent, direction, new System.Numerics.Vector2(0, 0), args.Performer, args.Performer, 20f);

                _psionics.LogPowerUsed(args.Performer, "pyrokinesis", psionic, 6, 8);
                args.Handled = true;
            }
        }
    }
}
