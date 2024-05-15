using Content.Shared.Actions;
using Content.Shared.Abilities.Psionics;
using Content.Server.Atmos.Components;
using Content.Server.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Content.Shared.Actions.Events;
using Content.Server.Explosion.Components;

namespace Content.Server.Psionics.Abilities
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly GunSystem _gunSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly PhysicsSystem _physics = default!;

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
                psionic.Amplification += 1f;
            }
        }

        private void OnPowerUsed(PyrokinesisPowerActionEvent args)
        {
            if (!TryComp<PsionicComponent>(args.Performer, out var psionic))
                return;

            if (!HasComp<PsionicInsulationComponent>(args.Performer))
            {
                var ent = Spawn("ProjectileAnomalyFireball");

                if (TryComp<ExplosiveComponent>(ent, out var fireball))
                {
                    fireball.MaxIntensity = (int) MathF.Round(20 * psionic.Amplification - 10 * psionic.Dampening);

                    if (psionic.Amplification > 5 && EnsureComp<IgniteOnCollideComponent>(ent, out var ignite))
                    {
                        ignite.FireStacks = 0.1f * psionic.Amplification - 0.1f * psionic.Dampening;
                    }
                }

                var userVelocity = _physics.GetMapLinearVelocity(args.Performer);
                var direction = args.Target.ToMapPos(EntityManager, _transformSystem);

                _gunSystem.ShootProjectile(ent, direction, userVelocity, args.Performer, args.Performer, 20f);

                _psionics.LogPowerUsed(args.Performer, "pyrokinesis",
                    (int) MathF.Round(6f * psionic.Amplification - psionic.Dampening),
                    (int) MathF.Round(8f * psionic.Amplification - psionic.Dampening));
                args.Handled = true;
            }
        }
    }
}
