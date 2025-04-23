using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.DeviceLinking.Components
{
    /// <summary>
    /// A component with an On/Off state controlled by signals from DeviceLinking
    /// </summary>
    [RegisterComponent, Access(typeof(SignalControlSystem))]
    public sealed partial class SignalControlComponent : Component
    {
        [DataField]
        public ProtoId<SinkPortPrototype> TogglePort = "Toggle";

        [DataField]
        public ProtoId<SinkPortPrototype> OnPort = "On";

        [DataField]
        public ProtoId<SinkPortPrototype> OffPort = "Off";

        [DataField("isOnByDefault")]
        public bool IsOnByDefault = false;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool IsOn = false;
    }
}
