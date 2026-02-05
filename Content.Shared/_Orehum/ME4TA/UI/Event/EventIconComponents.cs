using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Orehum.ME4TA.UI.Event.Components
{
    [NetworkedComponent, RegisterComponent]
    public sealed partial class EventIconComponent : Component
    {
        [DataField("eventStatusIcon")]
        public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "EventFaction";
    }

    [NetworkedComponent, RegisterComponent]
    public sealed partial class ShowEventIconComponent : Component;
}
