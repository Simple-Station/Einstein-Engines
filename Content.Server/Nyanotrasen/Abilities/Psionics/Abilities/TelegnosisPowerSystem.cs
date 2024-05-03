using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.StatusEffect;
using Content.Shared.Abilities.Psionics;
using Content.Server.Mind.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Abilities.Psionics
{
    public sealed class TelegnosisPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly MindSwapPowerSystem _mindSwap = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<TelegnosisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<TelegnosisPowerComponent, TelegnosisPowerActionEvent>(OnPowerUsed);
            SubscribeLocalEvent<TelegnosticProjectionComponent, MindRemovedMessage>(OnMindRemoved);
        }

        private void OnInit(EntityUid uid, TelegnosisPowerComponent component, ComponentInit args)
        {
            if (!_prototypeManager.TryIndex<InstantActionPrototype>("Telegnosis", out var telegnosis))
                return;

            component.TelegnosisPowerAction = new InstantAction(telegnosis);
            if (telegnosis.UseDelay != null)
                component.TelegnosisPowerAction.Cooldown = (_gameTiming.CurTime, _gameTiming.CurTime + (TimeSpan) telegnosis.UseDelay);
            _actions.AddAction(uid, component.TelegnosisPowerAction, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.TelegnosisPowerAction;
        }

        private void OnShutdown(EntityUid uid, TelegnosisPowerComponent component, ComponentShutdown args)
        {
            if (_prototypeManager.TryIndex<InstantActionPrototype>("Telegnosis", out var metapsionic))
                _actions.RemoveAction(uid, new InstantAction(metapsionic), null);
        }

        private void OnPowerUsed(EntityUid uid, TelegnosisPowerComponent component, TelegnosisPowerActionEvent args)
        {
            var projection = Spawn(component.Prototype, Transform(uid).Coordinates);
            Transform(projection).AttachToGridOrMap();
            _mindSwap.Swap(uid, projection);

            _psionics.LogPowerUsed(uid, "telegnosis");
            args.Handled = true;
        }
        private void OnMindRemoved(EntityUid uid, TelegnosticProjectionComponent component, MindRemovedMessage args)
        {
            QueueDel(uid);
        }
    }

    public sealed class TelegnosisPowerActionEvent : InstantActionEvent {}
}
