using Content.Server.EventScheduler;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Cybernetics;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Cybernetics
{
    public sealed class BluespaceShuntSystem : EntitySystem
    {

        [Dependency] private readonly EventSchedulerSystem _eventScheduler = default!;

        [Dependency] private readonly SharedActionsSystem _actions = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        [Dependency] private readonly IGameTiming _timing = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BluespaceShuntComponent, BluespaceShuntUsedEvent>(OnBluespaceShuntUsed);
            SubscribeLocalEvent<BluespaceShuntComponent, BluespaceShuntCooldownEndEvent>(OnCooldownEnd);


            SubscribeLocalEvent<BluespaceShuntComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<BluespaceShuntComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnBluespaceShuntUsed(EntityUid uid, BluespaceShuntComponent component, ref BluespaceShuntUsedEvent args)
        {
            var ev = new BluespaceShuntCooldownEndEvent(component);
            _eventScheduler.ScheduleEvent(uid, ref ev, _timing.CurTime + TimeSpan.FromSeconds(component.CooldownTime));
        }

        private void OnCooldownEnd(EntityUid uid, BluespaceShuntComponent component, BluespaceShuntCooldownEndEvent args)
        {
            component.OnCooldown = false;
            Dirty(uid, component);
            _popup.PopupEntity(Loc.GetString("shunt-cooldown-end"), uid, uid);
        }

        private void OnStartup(EntityUid uid, BluespaceShuntComponent component, ComponentStartup args)
        {
            _actions.AddAction(uid, ref component.Action, component.ActionPrototype);
        }

        private void OnShutdown(EntityUid uid, BluespaceShuntComponent component, ComponentShutdown args)
        {
            _actions.RemoveAction(uid, component.Action);
        }
    }
}
