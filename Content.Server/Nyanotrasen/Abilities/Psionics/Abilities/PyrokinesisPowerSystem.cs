using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Content.Shared.Abilities.Psionics;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Popups;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Abilities.Psionics
{
    public sealed class PyrokinesisPowerSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly FlammableSystem _flammableSystem = default!;
        [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<PyrokinesisPowerComponent, ComponentShutdown>(OnShutdown);
            SubscribeLocalEvent<PyrokinesisPowerActionEvent>(OnPowerUsed);
        }

        private void OnInit(EntityUid uid, PyrokinesisPowerComponent component, ComponentInit args)
        {
            if (!_prototypeManager.TryIndex<EntityTargetActionPrototype>("Pyrokinesis", out var pyrokinesis))
                return;

            component.PyrokinesisPowerAction = new EntityTargetAction(pyrokinesis);
            if (pyrokinesis.UseDelay != null)
                component.PyrokinesisPowerAction.Cooldown = (_gameTiming.CurTime, _gameTiming.CurTime + (TimeSpan) pyrokinesis.UseDelay);
            _actions.AddAction(uid, component.PyrokinesisPowerAction, null);

            if (TryComp<PsionicComponent>(uid, out var psionic) && psionic.PsionicAbility == null)
                psionic.PsionicAbility = component.PyrokinesisPowerAction;
        }

        private void OnShutdown(EntityUid uid, PyrokinesisPowerComponent component, ComponentShutdown args)
        {
            if (_prototypeManager.TryIndex<EntityTargetActionPrototype>("Pyrokinesis", out var pyrokinesis))
                _actions.RemoveAction(uid, new EntityTargetAction(pyrokinesis), null);
        }

        private void OnPowerUsed(PyrokinesisPowerActionEvent args)
        {
            if (!TryComp<FlammableComponent>(args.Target, out var flammableComponent))
                return;

            flammableComponent.FireStacks += 5;
            _flammableSystem.Ignite(args.Target, flammableComponent);
            _popupSystem.PopupEntity(Loc.GetString("pyrokinesis-power-used", ("target", args.Target)), args.Target, Shared.Popups.PopupType.LargeCaution);

            _psionics.LogPowerUsed(args.Performer, "pyrokinesis");
            args.Handled = true;
        }
    }

    public sealed class PyrokinesisPowerActionEvent : EntityTargetActionEvent {}
}
