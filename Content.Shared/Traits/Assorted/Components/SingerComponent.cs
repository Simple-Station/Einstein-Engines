using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SingerComponent : Component
{
    [DataField(serverOnly: true)]
    public EntProtoId MidiActionId = "ActionHarpyPlayMidi";

    [DataField(serverOnly: true)] // server only, as it uses a server-BUI event !type
    public EntityUid? MidiAction;

    [DataField, AutoNetworkedField]
    public PrototypeData? MidiUi; // Traits are server-side so it needs to be replicated to the client
}
