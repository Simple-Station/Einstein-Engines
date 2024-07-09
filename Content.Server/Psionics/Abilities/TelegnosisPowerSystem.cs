using Content.Shared.Actions;
using Content.Shared.Psionics.Abilities;
using Content.Shared.Mind.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Mobs;
using Content.Shared.Storage.Components;

namespace Content.Server.Psionics.Abilities
{
    public sealed class TelegnosisPowerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwap = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<TelegnosisPowerComponent, TelegnosisPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<TelegnosticProjectionComponent, MindRemovedMessage>(OnMindRemoved);
            SubscribeLocalEvent<TelegnosisPowerComponent, DispelledEvent>(OnDispelled);
            SubscribeLocalEvent<TelegnosisPowerComponent, MobStateChangedEvent>(OnMobstateChanged);
            SubscribeLocalEvent<TelegnosisPowerComponent, InsertIntoEntityStorageAttemptEvent>(OnStorageInsertAttempt);
        }

        private void OnInit(EntityUid uid, TelegnosisPowerComponent component, ComponentInit args)
        {
            EnsureComp<PsionicComponent>(uid, out var psionic);
            _actions.AddAction(uid, ref component.TelegnosisActionEntity, component.TelegnosisActionId );
            _actions.TryGetActionData( component.TelegnosisActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.TelegnosisActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));

            psionic.ActivePowers.Add(component);
            psionic.PsychicFeedback.Add(component.TelegnosisFeedback);
            psionic.Amplification += 0.3f;
            psionic.Dampening += 0.3f;
        }

        private void OnShutdown(EntityUid uid, TelegnosisPowerComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.TelegnosisActionEntity);
            if (TryComp<PsionicComponent>(uid, out var psionic))
            {
                psionic.ActivePowers.Remove(component);
                psionic.PsychicFeedback.Remove(component.TelegnosisFeedback);
                psionic.Amplification -= 0.3f;
                psionic.Dampening -= 0.3f;
            }
        }

        private void OnPowerUsed(EntityUid uid, TelegnosisPowerComponent component, TelegnosisPowerActionEvent args)
        {
            if (!TryComp<PsionicComponent>(uid, out var psionic))
                return;

            if (HasComp<PsionicInsulationComponent>(uid))
                return;

            var projection = Spawn(component.Prototype, Transform(uid).Coordinates);
            Transform(projection).AttachToGridOrMap();
            component.OriginalEntity = uid;
            component.IsProjecting = true;
            component.ProjectionUid = projection;
            _mindSwap.Swap(uid, projection);

            _actions.TryGetActionData( component.TelegnosisActionEntity, out var actionData );
            if (actionData is { UseDelay: not null })
                _actions.SetCooldown(component.TelegnosisActionEntity, actionData.UseDelay.Value - TimeSpan.FromSeconds(psionic.Dampening + psionic.Amplification));

            if (EnsureComp<TelegnosticProjectionComponent>(projection, out var projectionComponent))
                projectionComponent.OriginalEntity = uid;

            _psionics.LogPowerUsed(uid, "telegnosis", psionic, 8, 12);

            args.Handled = true;
        }
        private void OnMindRemoved(EntityUid uid, TelegnosticProjectionComponent component, MindRemovedMessage args)
        {
            if (TryComp<TelegnosisPowerComponent>(component.OriginalEntity, out var originalEntity))
                originalEntity.IsProjecting = false;

            QueueDel(uid);
        }

        private void OnDispelled(EntityUid uid, TelegnosisPowerComponent component, DispelledEvent args)
        {
            if (component.IsProjecting)
                _mindSwap.Swap(uid, component.ProjectionUid);
        }

        private void OnMobstateChanged(EntityUid uid, TelegnosisPowerComponent component, MobStateChangedEvent args)
        {
            if (component.IsProjecting && args.NewMobState is MobState.Critical
            || component.IsProjecting && args.NewMobState is MobState.Dead)
                _mindSwap.Swap(uid, component.ProjectionUid);
        }

        private void OnStorageInsertAttempt(EntityUid uid, TelegnosisPowerComponent component, InsertIntoEntityStorageAttemptEvent args)
        {
            if (component.IsProjecting)
                _mindSwap.Swap(uid, component.ProjectionUid);
        }
    }
}
