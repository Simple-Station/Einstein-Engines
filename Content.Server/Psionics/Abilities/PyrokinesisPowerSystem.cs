using Content.Shared.Actions;
using Content.Shared.Psionics.Abilities;
using Content.Server.Atmos.Components;
using Content.Server.Weapons.Ranged.Systems;
using Robust.Server.GameObjects;
using Content.Shared.Actions.Events;
using Content.Server.Explosion.Components;
using Content.Shared.Mobs.Components;
using Robust.Shared.Map;

namespace Content.Server.Psionics.Abilities
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly TransformSystem _xform = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly GunSystem _gunSystem = default!;
        [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
        [Dependency] private readonly PhysicsSystem _physics = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
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
            if (!TryComp<PsionicComponent>(args.Performer, out var psionic))
                return;

            if (!HasComp<PsionicInsulationComponent>(args.Performer))
            {
                var xformQuery = GetEntityQuery<TransformComponent>();
                var xform = xformQuery.GetComponent(args.Performer);

                var mapPos = xform.Coordinates.ToMap(EntityManager, _xform);
                var spawnCoords = _mapManager.TryFindGridAt(mapPos, out var gridUid, out _)
                    ? xform.Coordinates.WithEntityId(gridUid, EntityManager)
                    : new(_mapManager.GetMapEntityId(mapPos.MapId), mapPos.Position);

                var ent = Spawn("ProjectileAnomalyFireball", spawnCoords);

                if (TryComp<ExplosiveComponent>(ent, out var fireball))
                {
                    fireball.MaxIntensity = (int) MathF.Round(20 * psionic.Amplification - 10 * psionic.Dampening);

                    if (psionic.Amplification > 5 && EnsureComp<IgniteOnCollideComponent>(ent, out var ignite))
                    {
                        ignite.FireStacks = 0.2f * psionic.Amplification - 0.1f * psionic.Dampening;
                    }
                }

                var direction = args.Target.Position;

                _gunSystem.ShootProjectile(ent, direction, new System.Numerics.Vector2(0, 0), args.Performer, args.Performer, 20f);

                _psionics.LogPowerUsed(args.Performer, "pyrokinesis",
                    (int) MathF.Round(6f * psionic.Amplification - psionic.Dampening),
                    (int) MathF.Round(8f * psionic.Amplification - psionic.Dampening));
                args.Handled = true;
            }
        }
    }
}
