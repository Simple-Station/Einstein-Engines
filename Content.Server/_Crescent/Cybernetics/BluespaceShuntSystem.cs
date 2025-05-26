using Content.Server.EventScheduler;
using Content.Shared.Cybernetics;
using Robust.Shared.GameObjects;

namespace Content.Server.Cybernetics
{
    public sealed class BluespaceShuntSystem : EntitySystem
    {

        [Dependency] private readonly EventSchedulerSystem _eventScheduler = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BluespaceShuntComponent, BluespaceShuntUsedEvent>(OnBluespaceShuntUsed);
        }

        private void OnBluespaceShuntUsed(EntityUid uid, BluespaceShuntComponent component, ref BluespaceShuntUsedEvent args)
        {
            var ev = new BluespaceShuntCooldownEndEvent(component);

            _eventScheduler.ScheduleEvent(uid, ref ev, TimeSpan.FromSeconds(component.CooldownTime));
        }
    }
}
