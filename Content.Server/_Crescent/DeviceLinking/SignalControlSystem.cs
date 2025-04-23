using Content.Server.DeviceLinking.Components;
using Content.Server.DeviceLinking.Events;

namespace Content.Server.DeviceLinking.Systems
{
    public sealed class SignalControlSystem : EntitySystem
    {
        [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SignalControlComponent, MapInitEvent>(OnInit);
            SubscribeLocalEvent<SignalControlComponent, SignalReceivedEvent>(OnSignalReceived);
        }

        private void OnInit(EntityUid uid, SignalControlComponent control, ref MapInitEvent args)
        {
            _signalSystem.EnsureSinkPorts(uid, control.TogglePort, control.OnPort, control.OffPort);
            control.IsOn = control.IsOnByDefault;
        }

        private void OnSignalReceived(EntityUid uid, SignalControlComponent control, ref SignalReceivedEvent args)
        {
            if (args.Port == control.TogglePort)
            {
                control.IsOn = !control.IsOn;
            }
            else if (args.Port == control.OnPort)
            {
                control.IsOn = true;
            }
            else if (args.Port == control.OffPort)
            {
                control.IsOn = false;
            }
        }
    }
}
